using System;
using System.Collections.Generic;

namespace Litium.Accelerator.Search
{ /// <summary>
  /// Helper class for the term count of a Tag
  /// </summary>
    public class TagTerms : IComparable<TagTerms>
    {
        /// <summary>
        /// Gets or sets the name of the tag.
        /// </summary>
        /// <value>The name of the tag.</value>
        public string TagName { get; set; }

        /// <summary>
        /// Gets or sets the term counts.
        /// </summary>
        /// <value>The term counts.</value>
		public List<TermCount> TermCounts { get; set; }

        /// <summary>
        /// Compares TagNames (ascending).
        /// </summary>
        /// <param name="other">The other.</param>
        /// <returns></returns>
        public int CompareTo(TagTerms other)
        {
            return String.Compare(TagName, other.TagName, StringComparison.Ordinal);
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return TagName;
        }
    }

    /// <summary>
    /// Term of a tag and the frequency it has.
    /// </summary>
    public class TermCount : IComparable<TermCount>
    {
        /// <summary>
        /// Gets the text.
        /// </summary>
        /// <value>The text.</value>
		public string Term { get; set; }
        /// <summary>
        /// Gets the frequency.
        /// </summary>
        /// <value>The frequency.</value>
		public int Count { get; set; }

        /// <summary>
        /// Gets ids that the term was found in.
        /// </summary>
        /// <value>The Hits id.</value>
        public IEnumerable<TermCountHit> Hits { get; set; }

        /// <summary>
        /// Compares Frequency (Descending), if same compare Text (ascending). 
        /// </summary>
        /// <param name="other">The other.</param>
        /// <returns></returns>
        public int CompareTo(TermCount other)
        {
            var c = -Count.CompareTo(other.Count);
            if (c == 0)
                return String.Compare(Term, other.Term, StringComparison.Ordinal);
            return c;
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return string.Format("{0} : {1}", Term, Count);
        }

        /// <summary>
        /// Hit info
        /// </summary>
        public class TermCountHit
        {
            /// <summary>
            /// Gets or sets the name of the index.
            /// </summary>
            /// <value>The name of the index.</value>
			public string IndexName { get; set; }
            /// <summary>
            /// Gets or sets the ID.
            /// </summary>
            /// <value>The ID.</value>
			public string ID { get; set; }

            /// <summary>
            /// Returns a <see cref="System.String"/> that represents this instance.
            /// </summary>
            /// <returns>
            /// A <see cref="System.String"/> that represents this instance.
            /// </returns>
            public override string ToString()
            {
                return string.Format("{0} : {1}", IndexName, ID);
            }
        }
    }
}
