using System;
using System.Linq;
using System.Windows.Forms;
using SSMSSQLComplete.Core.Snippets;

namespace SSMSSQLComplete.UI.Dialogs
{
    public partial class SnippetManagerDialog : Form
    {
        private readonly SnippetManager _snippetManager;

        public SnippetManagerDialog()
        {
            InitializeComponent();
            _snippetManager = SnippetManager.Instance;
            LoadSnippets();
        }

        private void InitializeComponent()
        {
            this.snippetsListView = new ListView();
            this.nameTextBox = new TextBox();
            this.shortcutTextBox = new TextBox();
            this.categoryTextBox = new TextBox();
            this.descriptionTextBox = new TextBox();
            this.codeTextBox = new TextBox();
            this.addButton = new Button();
            this.updateButton = new Button();
            this.deleteButton = new Button();
            this.closeButton = new Button();
            this.SuspendLayout();

            //
            // snippetsListView
            //
            this.snippetsListView.FullRowSelect = true;
            this.snippetsListView.GridLines = true;
            this.snippetsListView.Location = new System.Drawing.Point(12, 12);
            this.snippetsListView.Name = "snippetsListView";
            this.snippetsListView.Size = new System.Drawing.Size(300, 400);
            this.snippetsListView.TabIndex = 0;
            this.snippetsListView.View = View.Details;
            this.snippetsListView.Columns.Add("Name", 150);
            this.snippetsListView.Columns.Add("Shortcut", 100);
            this.snippetsListView.SelectedIndexChanged += SnippetsListView_SelectedIndexChanged;

            //
            // nameTextBox
            //
            this.nameTextBox.Location = new System.Drawing.Point(320, 30);
            this.nameTextBox.Name = "nameTextBox";
            this.nameTextBox.Size = new System.Drawing.Size(400, 20);
            this.nameTextBox.TabIndex = 1;

            //
            // shortcutTextBox
            //
            this.shortcutTextBox.Location = new System.Drawing.Point(320, 70);
            this.shortcutTextBox.Name = "shortcutTextBox";
            this.shortcutTextBox.Size = new System.Drawing.Size(200, 20);
            this.shortcutTextBox.TabIndex = 2;

            //
            // categoryTextBox
            //
            this.categoryTextBox.Location = new System.Drawing.Point(520, 70);
            this.categoryTextBox.Name = "categoryTextBox";
            this.categoryTextBox.Size = new System.Drawing.Size(200, 20);
            this.categoryTextBox.TabIndex = 3;

            //
            // descriptionTextBox
            //
            this.descriptionTextBox.Location = new System.Drawing.Point(320, 110);
            this.descriptionTextBox.Multiline = true;
            this.descriptionTextBox.Name = "descriptionTextBox";
            this.descriptionTextBox.Size = new System.Drawing.Size(400, 60);
            this.descriptionTextBox.TabIndex = 4;

            //
            // codeTextBox
            //
            this.codeTextBox.Font = new System.Drawing.Font("Consolas", 10F);
            this.codeTextBox.Location = new System.Drawing.Point(320, 190);
            this.codeTextBox.Multiline = true;
            this.codeTextBox.Name = "codeTextBox";
            this.codeTextBox.ScrollBars = ScrollBars.Both;
            this.codeTextBox.Size = new System.Drawing.Size(400, 200);
            this.codeTextBox.TabIndex = 5;

            //
            // addButton
            //
            this.addButton.Location = new System.Drawing.Point(320, 400);
            this.addButton.Name = "addButton";
            this.addButton.Size = new System.Drawing.Size(90, 25);
            this.addButton.TabIndex = 6;
            this.addButton.Text = "Add";
            this.addButton.Click += AddButton_Click;

            //
            // updateButton
            //
            this.updateButton.Location = new System.Drawing.Point(420, 400);
            this.updateButton.Name = "updateButton";
            this.updateButton.Size = new System.Drawing.Size(90, 25);
            this.updateButton.TabIndex = 7;
            this.updateButton.Text = "Update";
            this.updateButton.Click += UpdateButton_Click;

            //
            // deleteButton
            //
            this.deleteButton.Location = new System.Drawing.Point(520, 400);
            this.deleteButton.Name = "deleteButton";
            this.deleteButton.Size = new System.Drawing.Size(90, 25);
            this.deleteButton.TabIndex = 8;
            this.deleteButton.Text = "Delete";
            this.deleteButton.Click += DeleteButton_Click;

            //
            // closeButton
            //
            this.closeButton.Location = new System.Drawing.Point(630, 400);
            this.closeButton.Name = "closeButton";
            this.closeButton.Size = new System.Drawing.Size(90, 25);
            this.closeButton.TabIndex = 9;
            this.closeButton.Text = "Close";
            this.closeButton.Click += (s, e) => this.Close();

            //
            // Labels
            //
            var nameLabel = new Label { Text = "Name:", Location = new System.Drawing.Point(320, 10), AutoSize = true };
            var shortcutLabel = new Label { Text = "Shortcut:", Location = new System.Drawing.Point(320, 50), AutoSize = true };
            var categoryLabel = new Label { Text = "Category:", Location = new System.Drawing.Point(520, 50), AutoSize = true };
            var descLabel = new Label { Text = "Description:", Location = new System.Drawing.Point(320, 90), AutoSize = true };
            var codeLabel = new Label { Text = "Code:", Location = new System.Drawing.Point(320, 170), AutoSize = true };

            //
            // SnippetManagerDialog
            //
            this.ClientSize = new System.Drawing.Size(740, 450);
            this.Controls.Add(this.snippetsListView);
            this.Controls.Add(nameLabel);
            this.Controls.Add(this.nameTextBox);
            this.Controls.Add(shortcutLabel);
            this.Controls.Add(this.shortcutTextBox);
            this.Controls.Add(categoryLabel);
            this.Controls.Add(this.categoryTextBox);
            this.Controls.Add(descLabel);
            this.Controls.Add(this.descriptionTextBox);
            this.Controls.Add(codeLabel);
            this.Controls.Add(this.codeTextBox);
            this.Controls.Add(this.addButton);
            this.Controls.Add(this.updateButton);
            this.Controls.Add(this.deleteButton);
            this.Controls.Add(this.closeButton);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SnippetManagerDialog";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Text = "Snippet Manager";
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private ListView snippetsListView;
        private TextBox nameTextBox;
        private TextBox shortcutTextBox;
        private TextBox categoryTextBox;
        private TextBox descriptionTextBox;
        private TextBox codeTextBox;
        private Button addButton;
        private Button updateButton;
        private Button deleteButton;
        private Button closeButton;

        private void LoadSnippets()
        {
            snippetsListView.Items.Clear();

            foreach (var snippet in _snippetManager.GetAllSnippets())
            {
                var item = new ListViewItem(snippet.Name);
                item.SubItems.Add(snippet.Shortcut);
                item.Tag = snippet;
                snippetsListView.Items.Add(item);
            }
        }

        private void SnippetsListView_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (snippetsListView.SelectedItems.Count > 0)
            {
                var snippet = snippetsListView.SelectedItems[0].Tag as Snippet;
                if (snippet != null)
                {
                    nameTextBox.Text = snippet.Name;
                    shortcutTextBox.Text = snippet.Shortcut;
                    categoryTextBox.Text = snippet.Category;
                    descriptionTextBox.Text = snippet.Description;
                    codeTextBox.Text = snippet.Code;
                }
            }
        }

        private void AddButton_Click(object sender, EventArgs e)
        {
            var snippet = new Snippet
            {
                Name = nameTextBox.Text,
                Shortcut = shortcutTextBox.Text,
                Category = categoryTextBox.Text,
                Description = descriptionTextBox.Text,
                Code = codeTextBox.Text
            };

            _snippetManager.AddSnippet(snippet);
            LoadSnippets();
            ClearFields();
        }

        private void UpdateButton_Click(object sender, EventArgs e)
        {
            if (snippetsListView.SelectedItems.Count > 0)
            {
                var snippet = snippetsListView.SelectedItems[0].Tag as Snippet;
                if (snippet != null)
                {
                    snippet.Name = nameTextBox.Text;
                    snippet.Shortcut = shortcutTextBox.Text;
                    snippet.Category = categoryTextBox.Text;
                    snippet.Description = descriptionTextBox.Text;
                    snippet.Code = codeTextBox.Text;

                    _snippetManager.UpdateSnippet(snippet);
                    LoadSnippets();
                }
            }
        }

        private void DeleteButton_Click(object sender, EventArgs e)
        {
            if (snippetsListView.SelectedItems.Count > 0)
            {
                var snippet = snippetsListView.SelectedItems[0].Tag as Snippet;
                if (snippet != null)
                {
                    var result = MessageBox.Show(
                        $"Are you sure you want to delete snippet '{snippet.Name}'?",
                        "Confirm Delete",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);

                    if (result == DialogResult.Yes)
                    {
                        _snippetManager.DeleteSnippet(snippet.Id);
                        LoadSnippets();
                        ClearFields();
                    }
                }
            }
        }

        private void ClearFields()
        {
            nameTextBox.Clear();
            shortcutTextBox.Clear();
            categoryTextBox.Clear();
            descriptionTextBox.Clear();
            codeTextBox.Clear();
        }
    }
}
