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
        if (inputCtrl != null) inputCtrl.OnTileClicked += HandleTileClicked;
    }

    void OnDisable()
    {
        if (inputCtrl != null) inputCtrl.OnTileClicked -= HandleTileClicked;
    }

    void HandleTileClicked(Vector2Int coord, Tile2D tile)
    {
        if (!runtime || !builder || !turns) return;

        var pieceAt = runtime.GetPiece(coord) as Piece;

        // nothing selected yet → only select current team’s piece
        if (_selected == null)
        {
            if (pieceAt != null && pieceAt.Team == turns.currentTeam)
                Select(pieceAt);
            return;
        }

        // clicked a piece
        if (pieceAt != null)
        {
            // reselect only if same team
            if (pieceAt.Team == turns.currentTeam)
                Select(pieceAt);
            return;
        }

        // clicked empty tile → move only if legal & affordable
        if (_legal.TryGetValue(coord, out var opt))
        {
            bool ok = MoveResolver.TryExecuteMove(_selected, coord, opt.cost, runtime, spendEnergy: true);
            if (ok)
            {
                var keep = _selected;
                ClearSelection();
                Select(keep); // refresh options with new position/energy
            }
            else
            {
                // optional feedback: not enough energy, etc.
            }
        }
        else
        {
            // clicked illegal tile → optional: ClearSelection();
        }
    }

    void Select(Piece p)
    {
        if (!p || !runtime || !builder || !turns) return;

        ClearSelection();
        _selected = p;

        // render selected
        highlighter.HighlightSelected(p.GridPos);

        // compute legal moves with costs (rules+mods+tiles)
        _legal.Clear();
        var options = MoveGenerator.GetMoves(p, runtime);
        foreach (var opt in options)
        {
            if (!runtime.InBounds(opt.dest)) continue;
            _legal[opt.dest] = opt;

            bool affordable = turns.energy >= opt.cost;
            highlighter.HighlightOption(opt.dest, opt.isCapture, affordable);
        }
    }

    public void ClearSelection()
    {
        highlighter.ClearAll();
        _legal.Clear();
        _selected = null;
    }

    // Call this when modifiers change on the selected piece to refresh highlights
    public void RefreshIfSelected(Piece p)
    {
        if (_selected == p)
        {
            var keep = _selected;
            ClearSelection();
            Select(keep);
        }
    }
}
