using ExCSS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace projOb
{
    public class NewsIterator
    {
        private readonly List<(Media media, IReportable reportable)> Reportables;
        private int IterationState;
        public NewsIterator(List<(Media media, IReportable reportable)> reportables)
        {
            Reportables = reportables;
            IterationState = 0;
        }
        public bool HasMore()
        {
            return IterationState < Reportables.Count;
        }
        public (Media media, IReportable reportable)? GetNext()
        {
            if (HasMore()) 
                return Reportables[IterationState++];
            return null;
        }

    }
    public class NewsGenerator
    {
        private NewsIterator Iterator;
        public NewsGenerator(List<Media> media, List<IReportable> reportables) 
        {
            List<(Media media, IReportable reportable)> list = new List<(Media media, IReportable reportable)>();

            for (int i = 0; i < media.Count; i++)
                for (int j = 0; j < reportables.Count; j++)
                    list.Add((media[i], reportables[j]));
            Iterator = new NewsIterator(list);
        }
        public string? GenerateTextNews()
        {
            if (Iterator.HasMore())
            {
                (Media media, IReportable reportable) = Iterator.GetNext().Value;
                return reportable.Accept(media);
            }
            return null;
        }
    }
}
