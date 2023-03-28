using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.DataVisualization.Charting;
using System.IO;
using System.Xml;

using System.Drawing;


namespace Takeoff
{
    public class ChartResult : FileResult
    {

        public ChartResult():base(@"image/png")
        {
        }

        public Chart Chart
        {
            get
            {
                if (_chart == null)
                {
                    _chart = new Chart();
                }
                return _chart;
            }
            set
            {
                _chart = value;
            }
        }
        private Chart _chart;

        public string TemplateXml { get; set; }

        private void LoadTemplate()
        {
            var chart = Chart;
            chart.Serializer.Content = SerializationContents.All;
            chart.Serializer.SerializableContent = string.Empty;
            chart.Serializer.IsTemplateMode = true;
            chart.Serializer.IsResetWhenLoading = false;
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.IgnoreComments = true;

            XmlReader reader = XmlReader.Create(new StringReader(TemplateXml), settings);
            chart.Serializer.Load(reader);
        }

 
        protected override void WriteFile(HttpResponseBase response)
        {
            if (TemplateXml.HasChars())
                LoadTemplate();

            var outputStream = response.OutputStream;
            using (var ms = new System.IO.MemoryStream())
            {
                Chart.SaveImage(ms, System.Web.UI.DataVisualization.Charting.ChartImageFormat.Png);
                ms.Position = 0;

                byte[] buffer = new byte[0x1000];
                while (true)
                {
                    int count = ms.Read(buffer, 0, 0x1000);
                    if (count == 0)
                    {
                        return;
                    }
                    outputStream.Write(buffer, 0, count);
                }
            }

        }

    }
}
