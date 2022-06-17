using System;
using System.Collections.Generic;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Newtonsoft.Json;
using WebApplication1;

namespace SignalIRServerTest.Models
{
    public class TestType
    {
        public TestType()
        {
            this.TestPage = new HashSet<TestPage>();
        }
    
        public int Id { get; set; }
        public string Title { get; set; }

        [System.Text.Json.Serialization.JsonIgnore]
        [JsonIgnore]
        public FontIcon Icon => Id switch
        {
            //one
            1 => new FontIcon
            {
                FontFamily = new FontFamily("Segoe MDL2 Assets"),
                Glyph = "\uE915"
            },
            //many
            2 => new FontIcon
            {
                FontFamily = new FontFamily("Segoe MDL2 Assets"),
                Glyph = "\uE762"
            },
            //text
            3 => new FontIcon
            {
                FontFamily = new FontFamily("Segoe MDL2 Assets"),
                Glyph = "\uE8D3"
            },
            _ => throw new ArgumentOutOfRangeException()
        };
    
        [System.Text.Json.Serialization.JsonIgnore]
        public virtual ICollection<TestPage> TestPage { get; set; }
    }
}