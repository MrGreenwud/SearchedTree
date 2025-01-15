#if UNITY_EDITOR

namespace Enigmatic.SearchedTrees
{
    public class SearchedTreeGroup : SearchedTree 
    {
        public SearchedTreeGroup() { }

        public SearchedTreeGroup(string value)
        {
            Value = value;
        }
    }
}

#endif