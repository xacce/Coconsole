namespace Coconsole
{
	public class Suggestion : ISearchResult
	{
		public string sourceText;
		public string visualText;
		public double distance { get; set; }
		public int index { get; set; }
	}
}