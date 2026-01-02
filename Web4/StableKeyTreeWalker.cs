using System.Buffers;

namespace Web4;

internal struct StableKeyTreeWalker()
{
    private string parentKey = string.Empty;
    private int keyCursor = 0;
    private int parentLength = 0;

    public void MoveNextKey() => keyCursor++;
    public string GetNextKey() => Keymaker.GetKey(parentKey, keyCursor++, parentLength);
    public bool IsNextKey(string key) => Keymaker.IsKey(key, parentKey, keyCursor++, parentLength);

    public void CreateNewGeneration(string key, int numberOfChildren)
    {
        parentKey = key;
        keyCursor = 0;
        parentLength = numberOfChildren;
    }

    public void ReturnToParent(string key, int cursor, int numberOfChildren)
    {
        parentKey = key;
        keyCursor = (cursor >> 1) + 1;
        parentLength = numberOfChildren;
    }

    public void Reset()
    {
        parentKey = string.Empty;
        parentLength = 0;
        keyCursor = 0;
    }
}