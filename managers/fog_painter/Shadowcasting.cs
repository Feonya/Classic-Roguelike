using Godot;
using System;
public class Shadowcasting
{
    public Shadowcasting(Func<int, int, bool> blocksLight, Action<int, int> setVisible, Func<int, int, int> getDistance)
    {
        _blocksLight = blocksLight;
        GetDistance = getDistance;
        _setVisible = setVisible;
    }

    public void Compute(Vector2I origin, int rangeLimit)
    {
        _setVisible(origin.X, origin.Y);
        for (uint octant = 0; octant < 8; octant++)
            Compute(octant, origin, rangeLimit, 1, new Slope(1, 1), new Slope(0, 1));
    }

    struct Slope // represents the slope Y/X as a rational number d y
    {
        public Slope(uint y, uint x) { Y = y; X = x; }

        public bool Greater(uint y, uint x) { return Y * x > X * y; }
        public bool GreaterOrEqual(uint y, uint x) { return Y * x >= X * y; }
        public bool Less(uint y, uint x) { return Y * x < X * y; }


        public readonly uint X, Y;
    }

    void Compute(uint octant, Vector2I origin, int rangeLimit, uint x, Slope top, Slope bottom)
    {
        for (; x <= (uint)rangeLimit; x++)
        {
            uint topY;
            if (top.X == 1)
            {
                topY = x;
            }
            else
            {
                topY = ((x * 2 - 1) * top.Y + top.X) / (top.X * 2);
                if (BlocksLight(x, topY, octant, origin))
                {
                    if (top.GreaterOrEqual(topY * 2 + 1, x * 2) && !BlocksLight(x, topY + 1, octant, origin)) topY++;
                }
                else
                {
                    uint ax = x * 2;
                    if (BlocksLight(x + 1, topY + 1, octant, origin)) ax++;
                    if (top.Greater(topY * 2 + 1, ax)) topY++;
                }
            }

            uint bottomY;
            if (bottom.Y == 0)
            {
                bottomY = 0;
            }
            else // bottom > 0
            {
                bottomY = ((x * 2 - 1) * bottom.Y + bottom.X) / (bottom.X * 2);
                if (bottom.GreaterOrEqual(bottomY * 2 + 1, x * 2) && BlocksLight(x, bottomY, octant, origin) &&
                   !BlocksLight(x, bottomY + 1, octant, origin))
                {
                    bottomY++;
                }
            }

            int wasOpaque = -1;
            for (uint y = topY; (int)y >= (int)bottomY; y--)
            {
                if (rangeLimit < 0 || GetDistance((int)x, (int)y) <= rangeLimit)
                {
                    bool isOpaque = BlocksLight(x, y, octant, origin);
                    bool isVisible =
                      isOpaque || ((y != topY || top.Greater(y * 4 - 1, x * 4 + 1)) && (y != bottomY || bottom.Less(y * 4 + 1, x * 4 - 1)));
                    if (isVisible) SetVisible(x, y, octant, origin);

                    if (x != rangeLimit)
                    {
                        if (isOpaque)
                        {
                            if (wasOpaque == 0)
                            {
                                uint nx = x * 2, ny = y * 2 + 1;
                                if (BlocksLight(x, y + 1, octant, origin)) nx--;
                                if (top.Greater(ny, nx))
                                {
                                    if (y == bottomY) { bottom = new Slope(ny, nx); break; }
                                    else Compute(octant, origin, rangeLimit, x + 1, top, new Slope(ny, nx));
                                }
                                else
                                {
                                    if (y == bottomY) return;
                                }
                            }
                            wasOpaque = 1;
                        }
                        else
                        {
                            if (wasOpaque > 0)
                            {
                                uint nx = x * 2, ny = y * 2 + 1;

                                if (BlocksLight(x + 1, y + 1, octant, origin)) nx++;
                                if (bottom.GreaterOrEqual(ny, nx)) return;
                                top = new Slope(ny, nx);
                            }
                            wasOpaque = 0;
                        }
                    }
                }
            }
            if (wasOpaque != 0) break;
        }
    }

    bool BlocksLight(uint x, uint y, uint octant, Vector2I origin)
    {
        uint nx = (uint)origin.X, ny = (uint)origin.Y;
        switch (octant)
        {
            case 0: nx += x; ny -= y; break;
            case 1: nx += y; ny -= x; break;
            case 2: nx -= y; ny -= x; break;
            case 3: nx -= x; ny -= y; break;
            case 4: nx -= x; ny += y; break;
            case 5: nx -= y; ny += x; break;
            case 6: nx += y; ny += x; break;
            case 7: nx += x; ny += y; break;
        }
        return _blocksLight((int)nx, (int)ny);
    }

    void SetVisible(uint x, uint y, uint octant, Vector2I origin)
    {
        uint nx = (uint)origin.X, ny = (uint)origin.Y;
        switch (octant)
        {
            case 0: nx += x; ny -= y; break;
            case 1: nx += y; ny -= x; break;
            case 2: nx -= y; ny -= x; break;
            case 3: nx -= x; ny -= y; break;
            case 4: nx -= x; ny += y; break;
            case 5: nx -= y; ny += x; break;
            case 6: nx += y; ny += x; break;
            case 7: nx += x; ny += y; break;
        }
        _setVisible((int)nx, (int)ny);
    }

    readonly Func<int, int, bool> _blocksLight;
    readonly Func<int, int, int> GetDistance;
    readonly Action<int, int> _setVisible;
}