using System.Collections.Generic;
using UnityEngine;

public class SelectionController : MonoBehaviour
{
    [Header("Refs")]
    public TileInputController inputCtrl;
    public TileHighlighter highlighter;
    public BoardRuntime runtime;
    public BoardBuilder2D builder;
    public TurnManager turns;

    private Piece _selected;
    private readonly Dictionary<Vector2Int, MoveOption> _legal = new();

    Vector2Int? _lastHover;       // last hovered tile
    int _lastPreview = int.MinValue; // last preview value we sent

    private Vector2Int _selectedOrigin;


    //piece visual

    public PieceDragVisual dragVisual;   
    public InputManager input;           

    void Awake()
    {
        if (!runtime) runtime = BoardRuntime.Instance;
        if (!builder) builder = runtime ? runtime.builder : FindFirstObjectByType<BoardBuilder2D>();
        if (!turns)   turns   = TurnManager.Instance;
        if (!highlighter) highlighter = FindFirstObjectByType<TileHighlighter>();
        if (!inputCtrl) inputCtrl = FindFirstObjectByType<TileInputController>();
        if (!dragVisual) dragVisual = FindFirstObjectByType<PieceDragVisual>();  
        if (!input)      input      = InputManager.Instance;                    
    }

    void OnEnable()
    {
        if (inputCtrl != null)
        {
            inputCtrl.OnTileClicked += HandleTileClicked;
            inputCtrl.OnTileHovered += HandleTileHovered;
        }
    }

    void OnDisable()
    {
        if (inputCtrl != null)
        {
            inputCtrl.OnTileClicked -= HandleTileClicked;
            inputCtrl.OnTileHovered -= HandleTileHovered;
        }
    }

   void HandleTileClicked(Vector2Int coord, Tile2D tile)
    {
        if (!runtime || !builder || !turns) return;

        var pieceAt = runtime.GetPiece(coord) as Piece;

        // Nothing selected yet → try select a friendly piece
        if (_selected == null)
        {
            if (pieceAt != null && pieceAt.Team == turns.currentTeam)
                Select(pieceAt);
            return;
        }

        // If clicking the same friendly piece (or its current cell) -> toggle off/deselect
        if (coord == _selected.GridPos)
        {
            ClearSelection();                 // show real piece again, hide ghost
            return;
        }

        // If clicking the original pickup cell -> cancel drag and deselect (no move/energy)
        if (coord == _selectedOrigin)
        {
            ClearSelection();                 // piece was already there; just drop it and stop dragging
            return;
        }

        // Clicking another friendly piece -> switch selection
        if (pieceAt != null && pieceAt.Team == turns.currentTeam)
        {
            Select(pieceAt);
            return;
        }

        // Clicking a legal destination -> perform move/attack and then deselect
        if (_legal.TryGetValue(coord, out var opt))
        {
            if (MoveResolver.TryExecuteMove(_selected, coord, opt.cost, runtime))
            {
                // After a successful move/attack, DO NOT reselect.
                ClearSelection();             // auto-drop; user must click again to pick it up
            }
            return;
        }

        // Otherwise: clicked somewhere illegal → no-op (still dragging)
    }


     void HandleTileHovered(Vector2Int? coord, Tile2D _)
    {
        if (turns == null) return;

        // 1) If not hovering a tile, do NOTHING (keep last preview)
        if (!coord.HasValue) return;

        // 2) Debounce by coord change
        if (_lastHover == coord) return;
        _lastHover = coord;

        // 3) Compute preview only when actually over a tile
        int preview = turns.energy;
        if (_selected != null && _legal.TryGetValue(coord.Value, out var opt))
            preview = Mathf.Clamp(turns.energy - opt.cost, 0, turns.maxEnergyPerTurn);

        if (preview != _lastPreview)
        {
            _lastPreview = preview;
            GameSignals.RaisePreviewEnergyChanged(preview, turns.maxEnergyPerTurn);
        }
    }

    void Select(Piece p)
    {
        if (!p || !runtime || !builder || !turns) return;

        // If we had another selected, restore its visibility before switching
        if (_selected != null) _selected.SetVisible(true);

        ClearSelection();          // clears highlights only, we reassign _selected below
        _selected = p;
        _selectedOrigin = p.GridPos;  

        highlighter.HighlightSelected(p.GridPos);

        _legal.Clear();
        var options = MoveGenerator.GetMoves(p, runtime);
        foreach (var opt in options)
        {
            if (!runtime.InBounds(opt.dest)) continue;
            _legal[opt.dest] = opt;
            bool affordable = turns.energy >= opt.cost;
            highlighter.HighlightOption(opt.dest, opt.isCapture, affordable);
        }

        _lastHover = null;
        _lastPreview = int.MinValue;
        GameSignals.RaisePreviewEnergyChanged(turns.energy, turns.maxEnergyPerTurn);

        // >>> NEW: show ghost starting at the piece's grid center
        if (dragVisual) dragVisual.ShowFromPiece(p, builder.GridToWorld(p.GridPos));

        // >>> NEW: hide the real piece while dragging
        p.SetVisible(false);
    }

    public void ClearSelection()
    {
        highlighter.ClearAll();
        _legal.Clear();

        // >>> NEW: if a piece was selected, make it visible again
        if (_selected != null) _selected.SetVisible(true);

        _selected = null;

        _lastHover = null;
        _lastPreview = int.MinValue;
        if (turns != null)
            GameSignals.RaisePreviewEnergyChanged(turns.energy, turns.maxEnergyPerTurn);

        // >>> NEW: hide ghost
        if (dragVisual) dragVisual.Hide();
    }
    
    void Update() 
    {
        if (dragVisual && dragVisual.gameObject.activeSelf && input != null)
            dragVisual.SetTargetFromScreen(input.MousePosition);
    }
}
