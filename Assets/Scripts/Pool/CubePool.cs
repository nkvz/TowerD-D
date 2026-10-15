using System.Collections.Generic;

public class CubePool
{
    private readonly Stack<Cube> _available = new Stack<Cube>();
    private readonly HashSet<Cube> _inPool = new HashSet<Cube>();
    private readonly Game _game;

    public CubePool(Game game)
    {
        _game = game;
    }

    public bool TryGet(out Cube cube)
    {
        if (_available.Count > 0)
        {
            cube = _available.Pop();
            _inPool.Remove(cube);
            cube.gameObject.SetActive(true);
            return true;
        }

        cube = null;
        return false;
    }

    public void Return(Cube cube)
    {
        if (cube == null || !_inPool.Add(cube))
            return;

        cube.ResetForPool();
        cube.transform.SetParent(_game.PoolRoot, false);
        cube.gameObject.SetActive(false);
        _available.Push(cube);
    }
}
