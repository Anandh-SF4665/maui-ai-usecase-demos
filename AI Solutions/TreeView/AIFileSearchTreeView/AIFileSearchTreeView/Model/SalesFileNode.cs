using System;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace AIFileSearchTreeView
{
    /// <summary>
    /// Extended FileManager model for sales-related files with AI search metadata
    /// </summary>
    public record SalesFileNode : FileManager
    {
        #region Fields
        private DateTime createdDate;
        private DateTime modifiedDate;
        private string? fileType; // Invoice, Contract, Report, etc.
        private string[]? keywords;
        private string[]? clientNames;
        private string? quarter;
        private decimal? invoiceAmount;
        private bool isMatched; // Used for parent node highlighting
        private bool isHighlighted; // Used for final match highlighting
        private bool isExpanded; // Used for TreeView expansion state
        private double matchConfidenceScore;
        private string searchPath = string.Empty; // Full path from root
        #endregion

        #region Properties

        public DateTime CreatedDate
        {
            get { return createdDate; }
            set { createdDate = value; RaisedOnPropertyChanged(nameof(CreatedDate)); }
        }

        public DateTime ModifiedDate
        {
            get { return modifiedDate; }
            set { modifiedDate = value; RaisedOnPropertyChanged(nameof(ModifiedDate)); }
        }

        public string? FileType
        {
            get { return fileType; }
            set { fileType = value; RaisedOnPropertyChanged(nameof(FileType)); }
        }

        public string[]? Keywords
        {
            get { return keywords; }
            set { keywords = value; RaisedOnPropertyChanged(nameof(Keywords)); }
        }

        public string[]? ClientNames
        {
            get { return clientNames; }
            set { clientNames = value; RaisedOnPropertyChanged(nameof(ClientNames)); }
        }

        public string? Quarter
        {
            get { return quarter; }
            set { quarter = value; RaisedOnPropertyChanged(nameof(Quarter)); }
        }

        public decimal? InvoiceAmount
        {
            get { return invoiceAmount; }
            set { invoiceAmount = value; RaisedOnPropertyChanged(nameof(InvoiceAmount)); }
        }

        public bool IsMatched
        {
            get { return isMatched; }
            set { isMatched = value; RaisedOnPropertyChanged(nameof(IsMatched)); }
        }

        public bool IsHighlighted
        {
            get { return isHighlighted; }
            set { isHighlighted = value; RaisedOnPropertyChanged(nameof(IsHighlighted)); }
        }

        public bool IsExpanded
        {
            get { return isExpanded; }
            set { isExpanded = value; RaisedOnPropertyChanged(nameof(IsExpanded)); }
        }

        public double MatchConfidenceScore
        {
            get { return matchConfidenceScore; }
            set { matchConfidenceScore = value; RaisedOnPropertyChanged(nameof(MatchConfidenceScore)); }
        }

        public string SearchPath
        {
            get { return searchPath; }
            set { searchPath = value; RaisedOnPropertyChanged(nameof(SearchPath)); }
        }

        #endregion

        #region Constructor

        public SalesFileNode()
        {
            CreatedDate = DateTime.Now;
            ModifiedDate = DateTime.Now;
            FileType = string.Empty;
            Keywords = Array.Empty<string>();
            ClientNames = Array.Empty<string>();
            Quarter = string.Empty;
            SubFiles = new ObservableCollection<FileManager>(new ObservableCollection<SalesFileNode>());
        }

        #endregion
    }
}
