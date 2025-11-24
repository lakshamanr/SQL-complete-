using System;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SSMSSQLComplete.UI.Dialogs
{
    public partial class ResultsViewerDialog : Form
    {
        private readonly DataTable _results;
        private DataGridView _gridView;
        private SplitContainer _splitContainer;
        private TextBox _detailView;
        private Label _statusLabel;

        public ResultsViewerDialog(DataTable results)
        {
            _results = results;
            InitializeComponent();
            LoadResults();
        }

        private void InitializeComponent()
        {
            this.Text = "Enhanced Results Viewer";
            this.Size = new Size(1000, 600);
            this.StartPosition = FormStartPosition.CenterScreen;

            // Status label
            _statusLabel = new Label
            {
                Dock = DockStyle.Bottom,
                Height = 25,
                Padding = new Padding(5),
                BackColor = SystemColors.Control,
                Text = "Ready"
            };

            // Split container for grid and detail view
            _splitContainer = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Vertical,
                SplitterDistance = 400
            };

            // DataGridView for results
            _gridView = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells,
                AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle { BackColor = Color.LightGray }
            };
            _gridView.CellClick += GridView_CellClick;
            _gridView.CellFormatting += GridView_CellFormatting;

            // Detail view for JSON/XML content
            _detailView = new TextBox
            {
                Dock = DockStyle.Fill,
                Multiline = true,
                ScrollBars = ScrollBars.Both,
                Font = new Font("Consolas", 10),
                ReadOnly = true,
                BackColor = Color.White
            };

            _splitContainer.Panel1.Controls.Add(_gridView);
            _splitContainer.Panel2.Controls.Add(_detailView);

            this.Controls.Add(_splitContainer);
            this.Controls.Add(_statusLabel);
        }

        private void LoadResults()
        {
            try
            {
                _gridView.DataSource = _results;

                // Detect and mark JSON/XML columns
                foreach (DataGridViewColumn column in _gridView.Columns)
                {
                    var columnName = column.Name;
                    var dataColumn = _results.Columns[columnName];

                    // Check if column might contain JSON
                    if (IsLikelyJsonColumn(dataColumn, _results))
                    {
                        column.DefaultCellStyle.ForeColor = Color.Blue;
                        column.DefaultCellStyle.Font = new Font(_gridView.Font, FontStyle.Italic);
                        column.HeaderText = $"📄 {columnName}"; // Add indicator
                    }
                }

                _statusLabel.Text = $"Showing {_results.Rows.Count} rows, {_results.Columns.Count} columns";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading results: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void GridView_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0)
                return;

            try
            {
                var cell = _gridView.Rows[e.RowIndex].Cells[e.ColumnIndex];
                var value = cell.Value?.ToString();

                if (string.IsNullOrWhiteSpace(value))
                {
                    _detailView.Text = "(NULL or empty)";
                    return;
                }

                // Try to format as JSON
                if (IsJson(value))
                {
                    var formatted = FormatJson(value);
                    _detailView.Text = formatted;
                    _detailView.ForeColor = Color.DarkBlue;
                }
                // Try to format as XML
                else if (IsXml(value))
                {
                    var formatted = FormatXml(value);
                    _detailView.Text = formatted;
                    _detailView.ForeColor = Color.DarkGreen;
                }
                else
                {
                    _detailView.Text = value;
                    _detailView.ForeColor = Color.Black;
                }

                _statusLabel.Text = $"Column: {_gridView.Columns[e.ColumnIndex].Name}, Row: {e.RowIndex + 1}, Length: {value.Length}";
            }
            catch (Exception ex)
            {
                _detailView.Text = $"Error displaying content: {ex.Message}";
            }
        }

        private void GridView_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.Value == null || e.Value == DBNull.Value)
            {
                e.Value = "(NULL)";
                e.CellStyle.ForeColor = Color.Gray;
                e.CellStyle.Font = new Font(e.CellStyle.Font, FontStyle.Italic);
                return;
            }

            var value = e.Value.ToString();

            // Truncate long values for grid display
            if (value.Length > 100)
            {
                e.Value = value.Substring(0, 97) + "...";
            }

            // Special formatting for JSON
            if (IsJson(value))
            {
                e.CellStyle.BackColor = Color.LightYellow;
            }
            // Special formatting for XML
            else if (IsXml(value))
            {
                e.CellStyle.BackColor = Color.LightGreen;
            }
        }

        private bool IsLikelyJsonColumn(DataColumn column, DataTable table)
        {
            if (table.Rows.Count == 0)
                return false;

            // Check column name
            var columnName = column.ColumnName.ToLower();
            if (columnName.Contains("json") || columnName.Contains("data") || columnName.Contains("metadata"))
            {
                // Verify first few rows
                var sampleSize = Math.Min(10, table.Rows.Count);
                var jsonCount = 0;

                for (int i = 0; i < sampleSize; i++)
                {
                    var value = table.Rows[i][column.ColumnName]?.ToString();
                    if (!string.IsNullOrWhiteSpace(value) && IsJson(value))
                    {
                        jsonCount++;
                    }
                }

                return jsonCount > sampleSize / 2; // More than 50% are JSON
            }

            return false;
        }

        private bool IsJson(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return false;

            text = text.Trim();
            return (text.StartsWith("{") && text.EndsWith("}")) ||
                   (text.StartsWith("[") && text.EndsWith("]"));
        }

        private bool IsXml(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return false;

            text = text.Trim();
            return text.StartsWith("<") && text.EndsWith(">");
        }

        private string FormatJson(string json)
        {
            try
            {
                var parsed = JToken.Parse(json);
                return parsed.ToString(Formatting.Indented);
            }
            catch
            {
                return json;
            }
        }

        private string FormatXml(string xml)
        {
            try
            {
                var doc = System.Xml.Linq.XDocument.Parse(xml);
                return doc.ToString();
            }
            catch
            {
                return xml;
            }
        }
    }
}
