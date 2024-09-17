using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace New_Tradegy.Library
{
	public class mn
	{
		// NF
		public static void calc_종목별일중평균(List<string> rL, List<List<string>> aV)
		{
			int[] c_id = new int[1]; // number of columns needed
			string[,] x = new string[1000, 1]; // array declaration
			c_id[0] = 5; // everyday amount dealed 

			string directory = @"C:\병신\data\일\";
			int nthfile = -1;
			int nrow;

			double average = 0.0;
			foreach (string stock in rL)
			{
				nthfile++;
				nrow = data_컬럼(directory + stock + ".txt", c_id, x);
				if (nrow < 0)
					continue;

				List<double> alist = new List<double>();

				if (nrow < 0)
				{
					average = 0.0;
					continue;
				}
				else if (nrow < 24)
				{
					double sum = 0.0;
					for (int k = 0; k < 24; k++)
						sum += Convert.ToDouble(x[k, 0]);

					average = sum / nrow;
				}
				else
				{
					// The last 24 Rows Extraction


					for (int k = nrow - 1; k > nrow - 25; k--)
						alist.Add(Convert.ToDouble(x[k, 0]));

					alist.Sort();

					// Use 20 data and Calcurate Average
					double sum = 0.0;
					for (int k = 2; k < alist.Count - 2; k++)
						sum += alist[k];

					if(alist.Count - 4 != 0)
						average = sum / (alist.Count - 4.0);
				}

				List<string> clist = new List<string>();
				alist.Clear();
				clist.Add(stock);
				clist.Add(average.ToString());
				aV.Add(clist);
			}
		}

		

		public static List<string> find_리스트중리스트(string name, List<List<string>> groupList)
		{
			foreach (var sublist in groupList)
			{
				if (sublist.Count < 1)
					return null;
				if (sublist[0] == name)
					return sublist;
			}
			return null;
		}

		public static int find_리스트중이름위치(string name, List<string> singleList)
		{
			int index = 0;
			foreach (string item in singleList)
			{
				if (item == name)
					return index;
				index++;
			}
			return -1;
		}

		public static int data_컬럼(string file, int[] c_id, string[,] x)
		{
			if (!File.Exists(file))
			{
				return -1;
			}


			Stream sf = System.IO.File.Open(file,
					 FileMode.Open,
					 FileAccess.Read,
					 FileShare.ReadWrite);
			StreamReader sr = new StreamReader(sf);

			string line;
			int nrow = 0;
			while ((line = sr.ReadLine()) != null)
			{
				//List<string> alist = new List<string>();

				string[] words = line.Split(' ');
				for (int k = 0; k < c_id.Length; k++)
				{
					x[nrow, k] = words[c_id[k]];
				}
				nrow++;
			}
			return nrow;
		}
	}
}
