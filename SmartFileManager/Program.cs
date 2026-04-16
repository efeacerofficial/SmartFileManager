using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace SmartFileManager
{
    public class MainForm : Form
    {
        // Renk Paleti (Modern Flat Tasarım)
        private Color bgColor = Color.FromArgb(243, 244, 246);
        private Color panelColor = Color.White;
        private Color primaryColor = Color.FromArgb(59, 130, 246); // Mavi
        private Color successColor = Color.FromArgb(16, 185, 129); // Yeşil
        private Color dangerColor = Color.FromArgb(239, 68, 68);   // Kırmızı
        private Color darkBg = Color.FromArgb(31, 41, 55);         // Terminal Arka Plan
        private Color textColor = Color.FromArgb(31, 41, 55);

        // UI Kontrolleri
        private TextBox txtSource;
        private TextBox txtTarget;
        private RichTextBox rtxtLog;

        public MainForm()
        {
            SetupUI();
        }

        private void SetupUI()
        {
            // Form Ayarları
            this.Text = "Akıllı Dosya Yöneticisi & Otomasyon";
            this.Size = new Size(900, 650);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = bgColor;
            this.Font = new Font("Segoe UI", 10F, FontStyle.Regular);
            this.ShowIcon = false;

            // Ana Kapsayıcı
            Panel mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20)
            };
            this.Controls.Add(mainPanel);

            // Başlık
            Label lblTitle = new Label
            {
                Text = "⚡ Akıllı Dosya Otomasyonu",
                Font = new Font("Segoe UI", 18F, FontStyle.Bold),
                ForeColor = textColor,
                AutoSize = true,
                Location = new Point(20, 20)
            };
            mainPanel.Controls.Add(lblTitle);

            // Üst Panel (Yol Seçimleri)
            Panel pathPanel = new Panel
            {
                BackColor = panelColor,
                Location = new Point(20, 70),
                Size = new Size(840, 120),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                Padding = new Padding(15)
            };
            mainPanel.Controls.Add(pathPanel);

            // Kaynak Klasör Seçimi
            Label lblSource = new Label { Text = "Kaynak Klasör:", Location = new Point(15, 20), AutoSize = true, Font = new Font("Segoe UI", 10F, FontStyle.Bold) };
            txtSource = new TextBox { Location = new Point(130, 17), Size = new Size(580, 30), ReadOnly = true, BackColor = bgColor };
            Button btnSource = CreateFlatButton("Gözat", primaryColor, new Point(720, 15), new Size(100, 30));
            btnSource.Click += (s, e) => SelectFolder(txtSource);

            // Hedef Klasör Seçimi
            Label lblTarget = new Label { Text = "Hedef Klasör:", Location = new Point(15, 70), AutoSize = true, Font = new Font("Segoe UI", 10F, FontStyle.Bold) };
            txtTarget = new TextBox { Location = new Point(130, 67), Size = new Size(580, 30), ReadOnly = true, BackColor = bgColor };
            Button btnTarget = CreateFlatButton("Gözat", primaryColor, new Point(720, 65), new Size(100, 30));
            btnTarget.Click += (s, e) => SelectFolder(txtTarget);

            pathPanel.Controls.AddRange(new Control[] { lblSource, txtSource, btnSource, lblTarget, txtTarget, btnTarget });

            // Orta Panel (Aksiyon Butonları)
            FlowLayoutPanel actionPanel = new FlowLayoutPanel
            {
                Location = new Point(20, 210),
                Size = new Size(840, 60),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                BackColor = bgColor
            };
            mainPanel.Controls.Add(actionPanel);

            Button btnExt = CreateFlatButton("📁 Uzantıya Göre Düzenle", successColor, Point.Empty, new Size(220, 45));
            btnExt.Margin = new Padding(0, 0, 15, 0);
            btnExt.Click += (s, e) => OrganizeByExtension();

            Button btnDate = CreateFlatButton("📅 Tarihe Göre Düzenle", successColor, Point.Empty, new Size(200, 45));
            btnDate.Margin = new Padding(0, 0, 15, 0);
            btnDate.Click += (s, e) => OrganizeByDate();

            Button btnClean = CreateFlatButton("🗑️ Boş Klasörleri Sil", dangerColor, Point.Empty, new Size(180, 45));
            btnClean.Click += (s, e) => CleanEmptyFolders();

            actionPanel.Controls.AddRange(new Control[] { btnExt, btnDate, btnClean });

            // Alt Panel (Log / Terminal Ekranı)
            Label lblLog = new Label { Text = "İşlem Kayıtları (Log):", Location = new Point(20, 280), AutoSize = true, Font = new Font("Segoe UI", 10F, FontStyle.Bold) };
            mainPanel.Controls.Add(lblLog);

            rtxtLog = new RichTextBox
            {
                Location = new Point(20, 310),
                Size = new Size(840, 270),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                BackColor = darkBg,
                ForeColor = Color.LightGray,
                Font = new Font("Consolas", 10F), // Terminal görünümü için Consolas
                ReadOnly = true,
                BorderStyle = BorderStyle.None
            };
            mainPanel.Controls.Add(rtxtLog);

            LogMessage("Sistem hazır. Lütfen kaynak ve hedef klasörleri seçin.", Color.Cyan);
        }

        // --- YARDIMCI UI METOTLARI ---
        private Button CreateFlatButton(string text, Color backColor, Point location, Size size)
        {
            Button btn = new Button
            {
                Text = text,
                BackColor = backColor,
                ForeColor = Color.White,
                Location = location,
                Size = size,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            return btn;
        }

        private void LogMessage(string message, Color color)
        {
            rtxtLog.SelectionStart = rtxtLog.TextLength;
            rtxtLog.SelectionLength = 0;
            rtxtLog.SelectionColor = color;
            rtxtLog.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}\n");
            rtxtLog.SelectionColor = rtxtLog.ForeColor;
            rtxtLog.ScrollToCaret();
        }

        private void SelectFolder(TextBox targetTextBox)
        {
            using (FolderBrowserDialog fbd = new FolderBrowserDialog())
            {
                if (fbd.ShowDialog() == DialogResult.OK)
                {
                    targetTextBox.Text = fbd.SelectedPath;
                    LogMessage($"Klasör seçildi: {fbd.SelectedPath}", Color.LightGreen);
                }
            }
        }

        private bool ValidatePaths()
        {
            if (string.IsNullOrWhiteSpace(txtSource.Text) || string.IsNullOrWhiteSpace(txtTarget.Text))
            {
                MessageBox.Show("Lütfen hem kaynak hem de hedef klasörü seçin!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            if (!Directory.Exists(txtSource.Text))
            {
                MessageBox.Show("Kaynak klasör bulunamadı!", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            return true;
        }

        // --- İŞ MANTIĞI (BUSINESS LOGIC) ---

        private void OrganizeByExtension()
        {
            if (!ValidatePaths()) return;

            string source = txtSource.Text;
            string target = txtTarget.Text;

            // Dosya türleri kategorileri
            var categories = new Dictionary<string, string[]>
            {
                { "Resimler", new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".svg" } },
                { "Belgeler", new[] { ".pdf", ".doc", ".docx", ".txt", ".xlsx", ".xls", ".pptx" } },
                { "Videolar", new[] { ".mp4", ".avi", ".mkv", ".mov" } },
                { "Müzikler", new[] { ".mp3", ".wav", ".flac" } },
                { "Arşivler", new[] { ".zip", ".rar", ".7z", ".tar", ".gz" } },
                { "Uygulamalar", new[] { ".exe", ".msi", ".apk" } }
            };

            LogMessage("Uzantıya göre sınıflandırma başlıyor...", Color.Yellow);

            try
            {
                string[] files = Directory.GetFiles(source);
                int movedCount = 0;

                foreach (string file in files)
                {
                    string ext = Path.GetExtension(file).ToLower();
                    string categoryFolder = "Digerleri"; // Varsayılan

                    // Uzantıyı kategorilerde ara
                    foreach (var category in categories)
                    {
                        if (category.Value.Contains(ext))
                        {
                            categoryFolder = category.Key;
                            break;
                        }
                    }

                    string destDir = Path.Combine(target, categoryFolder);
                    Directory.CreateDirectory(destDir);

                    string fileName = Path.GetFileName(file);
                    string destFile = Path.Combine(destDir, fileName);

                    // Eğer aynı isimde dosya varsa üzerine yazmamak için (1) ekle
                    int count = 1;
                    string tempFileName = fileName;
                    while (File.Exists(destFile))
                    {
                        string nameOnly = Path.GetFileNameWithoutExtension(fileName);
                        tempFileName = $"{nameOnly} ({count}){ext}";
                        destFile = Path.Combine(destDir, tempFileName);
                        count++;
                    }

                    File.Move(file, destFile);
                    LogMessage($"Taşındı: {tempFileName} -> {categoryFolder}", Color.White);
                    movedCount++;
                }

                LogMessage($"İşlem tamamlandı. {movedCount} dosya taşındı.", Color.LightGreen);
            }
            catch (Exception ex)
            {
                LogMessage($"Hata: {ex.Message}", Color.Red);
            }
        }

        private void OrganizeByDate()
        {
            if (!ValidatePaths()) return;

            string source = txtSource.Text;
            string target = txtTarget.Text;

            LogMessage("Tarihe göre sınıflandırma başlıyor...", Color.Yellow);

            try
            {
                string[] files = Directory.GetFiles(source);
                int movedCount = 0;

                foreach (string file in files)
                {
                    DateTime creationTime = File.GetCreationTime(file);
                    string yearMonthFolder = Path.Combine(creationTime.Year.ToString(), creationTime.ToString("MM-MMMM")); // Örn: 2023\04-Nisan

                    string destDir = Path.Combine(target, yearMonthFolder);
                    Directory.CreateDirectory(destDir);

                    string fileName = Path.GetFileName(file);
                    string destFile = Path.Combine(destDir, fileName);

                    if (!File.Exists(destFile))
                    {
                        File.Move(file, destFile);
                        LogMessage($"Taşındı: {fileName} -> {yearMonthFolder}", Color.White);
                        movedCount++;
                    }
                }

                LogMessage($"İşlem tamamlandı. {movedCount} dosya taşındı.", Color.LightGreen);
            }
            catch (Exception ex)
            {
                LogMessage($"Hata: {ex.Message}", Color.Red);
            }
        }

        private void CleanEmptyFolders()
        {
            if (string.IsNullOrWhiteSpace(txtTarget.Text) || !Directory.Exists(txtTarget.Text))
            {
                MessageBox.Show("Lütfen geçerli bir hedef klasör seçin!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            LogMessage("Boş klasör temizliği başlıyor...", Color.Yellow);
            int deletedCount = ProcessEmptyFolders(txtTarget.Text);
            LogMessage($"Temizlik tamamlandı. {deletedCount} boş klasör silindi.", Color.LightGreen);
        }

        private int ProcessEmptyFolders(string dirPath)
        {
            int count = 0;
            try
            {
                foreach (string d in Directory.GetDirectories(dirPath))
                {
                    count += ProcessEmptyFolders(d);
                }

                if (!Directory.EnumerateFileSystemEntries(dirPath).Any())
                {
                    Directory.Delete(dirPath);
                    LogMessage($"Silindi (Boş Klasör): {Path.GetFileName(dirPath)}", Color.Gray);
                    count++;
                }
            }
            catch (Exception ex)
            {
                LogMessage($"Klasör silinirken hata: {ex.Message}", Color.Red);
            }
            return count;
        }
    }

    // Projenin başlangıç noktası (Program.cs içindeki mantık, tek dosyada çalışması için eklendi)
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}