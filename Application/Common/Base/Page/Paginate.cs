using System.Collections;

namespace Application.Common.Base.Page
{
	public class Paginate<T> : IPaginate<T>
	{
		public int From { get; }
		public int Index { get; }
		public int Size { get; }
		public int Count { get; }
		public int Pages { get; }
		public IList<T> Items { get; }
		public bool HasPrevious => Index - From > 0;
		public bool HasNext => Index - From + 1 < Pages;

		public Paginate(IEnumerable<T> items, int count, int index, int size, int from)
		{
			if(from > index)
				throw new ArgumentException($"Index from {from} > index page {index}, must IndexFrom < IndexPage");

			this.Index = index;
			this.Size = size;
			this.From = from;
			this.Count = count;
			this.Items = [.. items];
			this.Pages = Convert.ToInt32(Math.Ceiling(count / Convert.ToDecimal(size)));
		}

		public IEnumerator<T> GetEnumerator() => Items.GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
	}
}
