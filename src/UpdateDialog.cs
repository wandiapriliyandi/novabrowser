using System;
using System.Drawing;
using System.Windows.Forms;

namespace NovaBrowser {
    public class UpdateDialog : Form {
        public bool ShouldUpdate { get; private set; }

        public UpdateDialog(string currentVer, string newVer, string changelog) {
            this.Text = "🎉 Pembaruan Tersedia - NovaLite Browser";
            this.Size = new Size(500, 320);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.FromArgb(15, 23, 42);
            this.ForeColor = Color.White;
            this.Font = new Font("Segoe UI", 9.5f);

            BuildUI(currentVer, newVer, changelog);
        }

        private void BuildUI(string currentVer, string newVer, string changelog) {
            TableLayoutPanel layout = new TableLayoutPanel() {
                Dock = DockStyle.Fill,
                Padding = new Padding(20),
                RowCount = 4,
                ColumnCount = 1
            };
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 45));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 35));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 45));

            Label lblTitle = new Label() {
                Text = "Versi baru NovaLite Browser telah tersedia!",
                Font = new Font("Segoe UI", 12f, FontStyle.Bold),
                ForeColor = Color.FromArgb(56, 189, 248),
                Dock = DockStyle.Fill
            };

            Label lblVer = new Label() {
                Text = string.Format("Versi Anda: {0}   ➔   Versi Terbaru: {1}", currentVer, newVer),
                ForeColor = Color.FromArgb(148, 163, 184),
                Dock = DockStyle.Fill
            };

            TextBox txtChangelog = new TextBox() {
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(30, 41, 59),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9.5f),
                Text = "Catatan Pembaruan:\r\n" + (string.IsNullOrWhiteSpace(changelog) ? "Peningkatan performa dan perbaikan bug." : changelog),
                ReadOnly = true
            };

            FlowLayoutPanel pnlButtons = new FlowLayoutPanel() {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft
            };

            Button btnUpdate = new Button() {
                Text = "⚡ Perbarui Sekarang",
                AutoSize = true,
                Height = 32,
                BackColor = Color.FromArgb(16, 185, 129),
                ForeColor = Color.Black,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold)
            };
            btnUpdate.Click += (s, e) => {
                ShouldUpdate = true;
                this.Close();
            };

            Button btnLater = new Button() {
                Text = "Nanti Saja",
                Width = 90,
                Height = 32,
                BackColor = Color.FromArgb(51, 65, 85),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnLater.Click += (s, e) => {
                ShouldUpdate = false;
                this.Close();
            };

            pnlButtons.Controls.Add(btnUpdate);
            pnlButtons.Controls.Add(btnLater);

            layout.Controls.Add(lblTitle, 0, 0);
            layout.Controls.Add(lblVer, 0, 1);
            layout.Controls.Add(txtChangelog, 0, 2);
            layout.Controls.Add(pnlButtons, 0, 3);

            this.Controls.Add(layout);
        }
    }
}

