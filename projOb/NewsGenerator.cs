using ExCSS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace projOb
{
    public class NewsGenerator
    {
        private (Media media, IReportable reportable)[] Reportables;
        private int IterationState;
        public NewsGenerator(List<Media> media, List<IReportable> reportables) 
        {
            Reportables = new (Media, IReportable)[media.Count * reportables.Count];
            int size = 0;
            for (int i = 0; i < media.Count; i++)
            {
                for (int j = 0; j < reportables.Count; j++)
                {
                    Reportables[size++] = (media[i], reportables[j]);
                }
            }
            IterationState = 0;
        }
        public bool HasMore()
        {
            return IterationState < Reportables.Length;
        }
        public string? GenerateTextNews()
        {
            if (HasMore())
            {
                IReportable reportable = Reportables[IterationState].reportable;
                Media media = Reportables[IterationState++].media;
                return reportable.Accept(media);
            }
            return null;
        }

    }
    
}
