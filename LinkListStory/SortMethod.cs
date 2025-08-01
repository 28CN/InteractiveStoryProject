using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkListStory
{
    public static class SortMethod
    {
        public static List<User> BubbleSort(List<User> users, string sortBy, bool ascending)
        {
            var sorted = new List<User>(users);
            for (int i = 0; i < sorted.Count - 1; i++)
            {
                for (int j = 0; j < sorted.Count - i - 1; j++)
                {
                    bool swap = false;
                    if (sortBy == "name")
                    {
                        int cmp = string.Compare(sorted[j].Name, sorted[j + 1].Name, StringComparison.OrdinalIgnoreCase);
                        swap = ascending ? cmp > 0 : cmp < 0;
                    }
                    else if (sortBy == "score")
                    {
                        swap = ascending ? sorted[j].Score > sorted[j + 1].Score : sorted[j].Score < sorted[j + 1].Score;
                    }

                    if (swap)
                    {
                        (sorted[j], sorted[j + 1]) = (sorted[j + 1], sorted[j]);
                    }
                }
            }
            return sorted;
        }
        public static List<User> InsertionSort(List<User> users, string sortBy, bool ascending)
        {
            var sorted = new List<User>(users);
            for (int i = 1; i < sorted.Count; i++)
            {
                int j = i;
                while (j > 0 && Compare(sorted[j - 1], sorted[j], sortBy, ascending))
                {
                    (sorted[j], sorted[j - 1]) = (sorted[j - 1], sorted[j]);
                    j--;
                }
            }
            return sorted;
        }

        public static List<User> SelectionSort(List<User> users, string sortBy, bool ascending)
        {
            var sorted = new List<User>(users);
            for (int i = 0; i < sorted.Count - 1; i++)
            {
                int targetIndex = i;
                for (int j = i + 1; j < sorted.Count; j++)
                {
                    bool condition = Compare(sorted[targetIndex], sorted[j], sortBy, ascending);
                    if (condition)
                    {
                        targetIndex = j;
                    }
                }

                if (targetIndex != i)
                {
                    (sorted[i], sorted[targetIndex]) = (sorted[targetIndex], sorted[i]);
                }
            }
            return sorted;
        }

        private static bool Compare(User a, User b, string sortBy, bool ascending)
        {
            if (sortBy == "name")
            {
                int cmp = string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase);
                return ascending ? cmp > 0 : cmp < 0;
            }
            else if (sortBy == "score")
            {
                return ascending ? a.Score > b.Score : a.Score < b.Score;
            }
            return false;
        }
    }
}