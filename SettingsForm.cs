using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using TrayDesk.Properties;

namespace TrayDesk
{
    /// <summary>
    /// Simple modal dialog for editing the user-scoped settings. Replaces hand-editing App.config.
    /// (ReportingSpan is intentionally omitted: it is application-scoped and must match the Arduino sketch.)
    /// </summary>
    public class SettingsForm : Form
    {
        private readonly TextBox _heightThreshold = new();
        private readonly TextBox _minUpShare = new();
        private readonly TextBox _daybreak = new();
        private readonly TextBox _dontWarnBefore = new();

        public SettingsForm()
        {
            Text = "TrayDesk Settings";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterScreen;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            AutoSize = true;
            AutoSizeMode = AutoSizeMode.GrowAndShrink;

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                Padding = new Padding(10),
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
            };

            AddRow(layout, "Height threshold (cm):", _heightThreshold);
            AddRow(layout, "Min up share (0-1):", _minUpShare);
            AddRow(layout, "Daybreak (hh:mm:ss):", _daybreak);
            AddRow(layout, "Don't warn before (hh:mm:ss):", _dontWarnBefore);

            var ok = new Button { Text = "OK", DialogResult = DialogResult.OK, AutoSize = true };
            var cancel = new Button { Text = "Cancel", DialogResult = DialogResult.Cancel, AutoSize = true };
            ok.Click += Ok_Click;

            var buttons = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.RightToLeft,
                Dock = DockStyle.Fill,
                AutoSize = true,
            };
            buttons.Controls.Add(cancel);
            buttons.Controls.Add(ok);
            layout.Controls.Add(buttons);
            layout.SetColumnSpan(buttons, 2);

            Controls.Add(layout);
            AcceptButton = ok;
            CancelButton = cancel;

            LoadValues();
        }

        private static void AddRow(TableLayoutPanel layout, string label, TextBox box)
        {
            box.Width = 120;
            box.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            layout.Controls.Add(new Label
            {
                Text = label,
                AutoSize = true,
                Anchor = AnchorStyles.Left,
                Margin = new Padding(3, 6, 3, 3),
            });
            layout.Controls.Add(box);
        }

        private void LoadValues()
        {
            _heightThreshold.Text = Settings.Default.HeightThreshold.ToString(CultureInfo.InvariantCulture);
            _minUpShare.Text = Settings.Default.MinUpShare.ToString(CultureInfo.InvariantCulture);
            _daybreak.Text = Settings.Default.Daybreak.ToString();
            _dontWarnBefore.Text = Settings.Default.DontWarnBefore.ToString();
        }

        private void Ok_Click(object sender, EventArgs e)
        {
            if (!int.TryParse(_heightThreshold.Text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var height) ||
                !double.TryParse(_minUpShare.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out var share) ||
                !TimeSpan.TryParse(_daybreak.Text, CultureInfo.InvariantCulture, out var daybreak) ||
                !TimeSpan.TryParse(_dontWarnBefore.Text, CultureInfo.InvariantCulture, out var dontWarnBefore))
            {
                MessageBox.Show("Please enter valid values.", "TrayDesk", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                DialogResult = DialogResult.None; // keep the dialog open
                return;
            }

            if (share is < 0 or > 1)
            {
                MessageBox.Show("Min up share must be between 0 and 1.", "TrayDesk", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                DialogResult = DialogResult.None;
                return;
            }

            Settings.Default.HeightThreshold = height;
            Settings.Default.MinUpShare = share;
            Settings.Default.Daybreak = daybreak;
            Settings.Default.DontWarnBefore = dontWarnBefore;
            Settings.Default.Save();
        }
    }
}
