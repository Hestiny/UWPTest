using App1.ViewModel.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App1.Collections
{
    class PinnedFolder
    {
        public PinnedFolder()
        {

        }

        public string Name = "Pinded";
        public enum PinnedType
        {
            Pin,
            Folder,
            File
        }
    }
}
