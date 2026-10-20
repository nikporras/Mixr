using System.Collections.Generic;

namespace Mixr.UI.Themes
{
    public class Ref
    {
        public string Key { get; set; }
        public string Value { get; set; }
        public List<Rule> Rules { get; } = new List<Rule>();

        public Ref() { }
    }
}