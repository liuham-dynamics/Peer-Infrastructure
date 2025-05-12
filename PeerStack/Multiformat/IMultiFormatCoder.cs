using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeerStack.Multiformat
{
   public interface IMultiFormatCoder
    {

        /// <summary>
        /// Specific multiformat coder or codec name
        /// </summary>
        public string? Name { get; }

        /// <summary>
        /// Multiformat type (e.g., multi-base or multi-codec)
        /// </summary>
        public MultiFormatType FormatType { get; }
    }
}
