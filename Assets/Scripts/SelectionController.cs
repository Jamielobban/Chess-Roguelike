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

    void Awake()
    {
        if (!runtime) runtime = BoardRuntime.Instance;
        if (!builder) builder = runtime ? runtime.builder : FindFirstObjectByType<BoardBuilder2D>();
        if (!turns)   turns   = TurnManager.Instance;
        if (!highlighter) highlighter = FindFirstObjectByType<TileHighlighter>();
        if (!inputCtrl)   inputCtrl   = FindFirstObjectByType<TileInputController>();
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

        if (_selected == null)
        {
            if (pieceAt != null && pieceAt.Team == turns.currentTeam) Select(pieceAt);
            return;
        }

        if (pieceAt != null && pieceAt.Team == turns.currentTeam)
        {
            Select(pieceAt);
            return;
        }

        if (_legal.TryGetValue(coord, out var opt))
        {
            if (MoveResolver.TryExecuteMove(_selected, coord, opt.cost, runtime))
            {
                var keep = _selected;
                ClearSelection();
                Select(keep);
            }
        }
    }

     void HandleTileHovered(Vector2Int? coord, Tile2D _)
    {
        if (turns == null) return;

        if (_lastHover == coord) return;
        _lastHover = coord;

        int preview = turns.energy;

        if (_selected != null && coord.HasValue && _legal.TryGetValue(coord.Value, out var opt))
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

        ClearSelection();
        _selected = p;

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
    }

    public void ClearSelection()
    {
        highlighter.ClearAll();
        _legal.Clear();
        _selected = null;
         _lastHover = null;
        _lastPreview = int.MinValue;
        if (turns != null)
            GameSignals.RaisePreviewEnergyChanged(turns.energy, turns.maxEnergyPerTurn);
    }
}
