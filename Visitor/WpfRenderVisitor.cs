using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows;
using TextEditorWpf.Core;
namespace Notepad.Visitor
{
    public class Visitor
    {
        public static System.Windows.Controls.Image Visit(ImageBlock img)
        {
            var res=new System.Windows.Controls.Image
            {
                Source = new BitmapImage(new Uri(img.Path, UriKind.Absolute)),
                Width = 200,
                Height = 150,
                Margin = new Thickness(5)
            };
            return res;
        }
        public static string Visit(TextBlock t){
            return t.Text;
        }
        
    }
}
