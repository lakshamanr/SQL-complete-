using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace SSMSSQLComplete.Core.Snippets
{
    public class SnippetRepository : ISnippetProvider
    {
        private readonly string _snippetsPath;
        private readonly List<Snippet> _snippets;

        public SnippetRepository()
        {
            _snippetsPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "SSMSSQLComplete",
                "Snippets",
                "snippets.json");

            _snippets = new List<Snippet>();
            LoadSnippets();
            EnsureDefaultSnippets();
        }

        public IEnumerable<Snippet> GetAllSnippets()
        {
            return _snippets.AsReadOnly();
        }

        public Snippet GetSnippet(string id)
        {
            return _snippets.FirstOrDefault(s => s.Id == id);
        }

        public Snippet GetSnippetByShortcut(string shortcut)
        {
            return _snippets.FirstOrDefault(s =>
                s.Shortcut.Equals(shortcut, StringComparison.OrdinalIgnoreCase));
        }

        public void AddSnippet(Snippet snippet)
        {
            if (snippet.Id == null)
                snippet.Id = Guid.NewGuid().ToString();

            _snippets.Add(snippet);
            SaveSnippets();
        }

        public void UpdateSnippet(Snippet snippet)
        {
            var existing = _snippets.FirstOrDefault(s => s.Id == snippet.Id);
            if (existing != null)
            {
                _snippets.Remove(existing);
                _snippets.Add(snippet);
                SaveSnippets();
            }
        }

        public void DeleteSnippet(string id)
        {
            var snippet = _snippets.FirstOrDefault(s => s.Id == id);
            if (snippet != null)
            {
                _snippets.Remove(snippet);
                SaveSnippets();
            }
        }

        public void SaveSnippets()
        {
            try
            {
                var directory = Path.GetDirectoryName(_snippetsPath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                var json = JsonConvert.SerializeObject(_snippets, Formatting.Indented);
                File.WriteAllText(_snippetsPath, json);

                Infrastructure.Logger.Instance.Info($"Saved {_snippets.Count} snippets");
            }
            catch (Exception ex)
            {
                Infrastructure.Logger.Instance.Error($"Error saving snippets: {ex.Message}", ex);
            }
        }

        private void LoadSnippets()
        {
            try
            {
                if (File.Exists(_snippetsPath))
                {
                    var json = File.ReadAllText(_snippetsPath);
                    var snippets = JsonConvert.DeserializeObject<List<Snippet>>(json);
                    if (snippets != null)
                    {
                        _snippets.AddRange(snippets);
                        Infrastructure.Logger.Instance.Info($"Loaded {_snippets.Count} snippets");
                    }
                }
            }
            catch (Exception ex)
            {
                Infrastructure.Logger.Instance.Error($"Error loading snippets: {ex.Message}", ex);
            }
        }

        private void EnsureDefaultSnippets()
        {
            if (_snippets.Any())
                return;

            // Add default snippets
            _snippets.AddRange(new[]
            {
                new Snippet
                {
                    Name = "Select All",
                    Shortcut = "sel",
                    Category = "DML",
                    Description = "SELECT * FROM table",
                    Code = "SELECT * FROM ${TableName}",
                    Parameters = new List<SnippetParameter>
                    {
                        new SnippetParameter("TableName", "YourTable", "Table name")
                    }
                },
                new Snippet
                {
                    Name = "Select Top",
                    Shortcut = "seltop",
                    Category = "DML",
                    Description = "SELECT TOP N columns FROM table",
                    Code = "SELECT TOP ${N}\n    ${Columns}\nFROM ${TableName}",
                    Parameters = new List<SnippetParameter>
                    {
                        new SnippetParameter("N", "100", "Number of rows"),
                        new SnippetParameter("Columns", "*", "Column list"),
                        new SnippetParameter("TableName", "YourTable", "Table name")
                    }
                },
                new Snippet
                {
                    Name = "Inner Join",
                    Shortcut = "innerjoin",
                    Category = "JOIN",
                    Description = "INNER JOIN template",
                    Code = "INNER JOIN ${Table2} ON ${Table1}.${Column1} = ${Table2}.${Column2}",
                    Parameters = new List<SnippetParameter>
                    {
                        new SnippetParameter("Table1", "Table1", "First table"),
                        new SnippetParameter("Table2", "Table2", "Second table"),
                        new SnippetParameter("Column1", "Id", "First column"),
                        new SnippetParameter("Column2", "Id", "Second column")
                    }
                },
                new Snippet
                {
                    Name = "Left Join",
                    Shortcut = "leftjoin",
                    Category = "JOIN",
                    Description = "LEFT JOIN template",
                    Code = "LEFT JOIN ${Table2} ON ${Table1}.${Column1} = ${Table2}.${Column2}",
                    Parameters = new List<SnippetParameter>
                    {
                        new SnippetParameter("Table1", "Table1", "First table"),
                        new SnippetParameter("Table2", "Table2", "Second table"),
                        new SnippetParameter("Column1", "Id", "First column"),
                        new SnippetParameter("Column2", "Id", "Second column")
                    }
                },
                new Snippet
                {
                    Name = "Insert Into",
                    Shortcut = "ins",
                    Category = "DML",
                    Description = "INSERT INTO template",
                    Code = "INSERT INTO ${TableName} (${Columns})\nVALUES (${Values})",
                    Parameters = new List<SnippetParameter>
                    {
                        new SnippetParameter("TableName", "YourTable", "Table name"),
                        new SnippetParameter("Columns", "Column1, Column2", "Column list"),
                        new SnippetParameter("Values", "'Value1', 'Value2'", "Values")
                    }
                },
                new Snippet
                {
                    Name = "Update",
                    Shortcut = "upd",
                    Category = "DML",
                    Description = "UPDATE template",
                    Code = "UPDATE ${TableName}\nSET ${Column} = ${Value}\nWHERE ${Condition}",
                    Parameters = new List<SnippetParameter>
                    {
                        new SnippetParameter("TableName", "YourTable", "Table name"),
                        new SnippetParameter("Column", "ColumnName", "Column to update"),
                        new SnippetParameter("Value", "'NewValue'", "New value"),
                        new SnippetParameter("Condition", "Id = 1", "WHERE condition")
                    }
                },
                new Snippet
                {
                    Name = "Create Table",
                    Shortcut = "crtbl",
                    Category = "DDL",
                    Description = "CREATE TABLE template",
                    Code = "CREATE TABLE ${TableName}\n(\n    ${Column1} INT PRIMARY KEY IDENTITY(1,1),\n    ${Column2} NVARCHAR(100) NOT NULL,\n    CreatedDate DATETIME DEFAULT GETDATE()\n)",
                    Parameters = new List<SnippetParameter>
                    {
                        new SnippetParameter("TableName", "YourTable", "Table name"),
                        new SnippetParameter("Column1", "Id", "Primary key column"),
                        new SnippetParameter("Column2", "Name", "Second column")
                    }
                }
            });

            SaveSnippets();
        }
    }
}
