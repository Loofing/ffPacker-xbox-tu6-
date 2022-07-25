using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ffPacker
{
	internal class file_tag
	{
		public string file_path { get; set; }
		public string asset_type { get; set; }
		public bool compress { get; set; }
		public string packaged_path { get; set; }

		public file_tag(string file_path, string asset_type, bool compress, string packaged_path)
		{
			this.file_path = file_path;
			this.asset_type = asset_type;
			this.compress = compress;
			this.packaged_path = packaged_path;
		}
	}
}
