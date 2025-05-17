
using System;
using System.Collections.Generic;

using TextEditorWpf.Core;
namespace TextEditorWpf.Core
{
    public class ImageBlock : DocumentElements
    {
        private string _path;

        public ImageBlock(string path)
        {
           _path = path;
            
        }
        public string Path
        {
            get => _path;
        }
        public override string Display()
        {
          
            return _path;
        }
        public override bool Equals(object obj)
        {
            if (obj is ImageBlock other)
            {
                return Path == other.Path;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return Path?.GetHashCode() ?? 0;
        }
    }
}
