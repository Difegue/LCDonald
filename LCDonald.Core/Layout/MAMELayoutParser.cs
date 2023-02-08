using System;
using System.Collections.Generic;
using System.Xml;

namespace LCDonald.Core.Layout
{

    /// <summary>
    /// Basic parser for XML MAME layout files.
    /// Only really handles elements, views and images.
    /// https://docs.mamedev.org/techspecs/layout_files.html
    /// </summary>
    public class MAMELayoutParser
    {

        public MAMELayoutParser()
        {
        }

        public MAMELayout Parse(string xmlPath)
        {
            var layout = new MAMELayout();
            var doc = new XmlDocument();
            doc.Load(xmlPath);

            var root = doc.DocumentElement;
            if (root.Name != "mamelayout")
            {
                throw new Exception("Root element must be 'mamelayout'");
            }

            foreach (System.Xml.XmlNode node in root.ChildNodes)
            {
                if (node.Name == "element")
                {
                    var element = ParseElement(node);
                    layout.Elements.Add(element.Name, element);
                }
                else if (node.Name == "view")
                {
                    var view = ParseView(node);
                    layout.Views.Add(view.Name, view);
                }
            }

            return layout;
        }

        private MAMEElement ParseElement(XmlNode node)
        {
            // If there's a child image node, make the matching MAMEImage object
            var imageNode = node.SelectSingleNode("image");
            var image = new MAMEImage
            {
                File = imageNode?.Attributes["file"]?.Value ?? ""
            };

            return new MAMEElement
            {
                Name = node.Attributes["name"].Value,
                Image = image
            };
        }

        private MAMEView ParseView(XmlNode node)
        {
            // If there's a child screen node, grab its bounds child node
            var screenBounds = node.SelectSingleNode("screen/bounds");
            
            var view = new MAMEView
            {
                Name = node.Attributes["name"].Value,
                ScreenHeight = screenBounds != null ? int.Parse(screenBounds.Attributes["height"].Value) : -1,
                ScreenWidth = screenBounds != null ? int.Parse(screenBounds.Attributes["width"].Value) : -1,
                ScreenX = screenBounds != null ? int.Parse(screenBounds.Attributes["x"].Value) : -1,
                ScreenY = screenBounds != null ? int.Parse(screenBounds.Attributes["y"].Value) : -1

            };
            view.Elements.AddRange(ParseViewElements(node));
            return view;
        }

        private IEnumerable<MAMEElementRef> ParseViewElements(XmlNode node)
        {
            var res = new List<MAMEElementRef>();

            // Look at all child "element" nodes and create MAMEElementRef objects out of them
            var elementNodes = node.SelectNodes("element");

            foreach (var element in elementNodes)
            {
                var boundsNode = ((XmlNode)element).SelectSingleNode("bounds");
                res.Add(new MAMEElementRef
                {
                    Ref = ((XmlNode)element).Attributes["ref"].Value,
                    InputTag = ((XmlNode)element).Attributes["inputtag"]?.Value,
                    Height = int.Parse(boundsNode.Attributes["height"].Value),
                    Width = int.Parse(boundsNode.Attributes["width"].Value),
                    X = int.Parse(boundsNode.Attributes["x"].Value),
                    Y = int.Parse(boundsNode.Attributes["y"].Value)
                });
            }

            return res;
        }
    }
}