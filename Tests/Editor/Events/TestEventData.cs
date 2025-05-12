namespace ArkSharp.Test
{
	public class TestEventData
	{
		public int iVal = 0;
		public string sVal = "";

		public void Inc0() => iVal++;
		public void Dec0() => iVal--;
		public void Inc1(int n) => iVal += n;
		public void Dec1(int n) => iVal -= n;
		public void Inc2(int n, string s) { iVal += n; sVal += s; }
		public void Dec2(int n, string s) { iVal -= n; sVal += s; }
	}
}
