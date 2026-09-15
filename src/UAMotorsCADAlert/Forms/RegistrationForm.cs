namespace UAMotorsCADAlert.Forms;

using System.Reflection;
using UAMotorsCADAlert.Services;

public class RegistrationForm : Form
{
    private TextBox _emailInput = null!;
    private Button _verifyButton = null!;
    private Label _statusLabel = null!;
    private string _rutaUAMOTORS;
    public bool IsRegistered { get; private set; }

    public RegistrationForm(string rutaUAMOTORS)
    {
        _rutaUAMOTORS = rutaUAMOTORS;
        InitializeComponent();
    }

    private string GetVersion()
    {
        try
        {
            var assembly = Assembly.GetExecutingAssembly();
            using var stream = assembly.GetManifestResourceStream("UAMotorsCADAlert.version.txt");
            if (stream != null)
            {
                using var reader = new StreamReader(stream);
                return reader.ReadToEnd().Trim();
            }
        }
        catch { }
        return "v2.0";
    }

    private void InitializeComponent()
    {
        this.Text = "UAMOTORS CAD ALERT - Registro";
        this.FormBorderStyle = FormBorderStyle.Sizable;
        this.MaximizeBox = true;
        this.MinimizeBox = false;
        this.StartPosition = FormStartPosition.CenterScreen;
        this.BackColor = Color.FromArgb(248, 250, 252);
        this.AutoScaleMode = AutoScaleMode.Dpi;

        this.Size = new Size(750, 550);
        this.MinimumSize = new Size(700, 500);
        this.AutoSize = true;
        this.AutoSizeMode = AutoSizeMode.GrowOnly;

        var mainLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            ColumnCount = 1,
            Padding = new Padding(35, 25, 35, 10)
        };
        mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
        this.Controls.Add(mainLayout);

        // Imagen de encabezado.
        try
        {
            var assembly = Assembly.GetExecutingAssembly();
            using var stream = assembly.GetManifestResourceStream("UAMotorsCADAlert.Resources.1.completo.png");
            if (stream != null && stream.Length > 0)
            {
                var bannerBox = new PictureBox
                {
                    Image = Image.FromStream(stream),
                    SizeMode = PictureBoxSizeMode.Zoom,
                    Height = 110, // Incremento de tamaño para mayor visibilidad.
                    Dock = DockStyle.Top,
                    BackColor = Color.Transparent,
                    Margin = new Padding(0, 0, 0, 15)
                };
                mainLayout.Controls.Add(bannerBox);
            }
        }
        catch (Exception) { }

        // Títulos.
        var titleBox = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            FlowDirection = FlowDirection.TopDown,
            AutoSize = true,
            WrapContents = false,
            Margin = new Padding(0, 0, 0, 15)
        };

        var titleLabel = new Label
        {
            Text = "UAMOTORS CAD ALERT",
            Font = new Font("Segoe UI", 18, FontStyle.Bold),
            ForeColor = Color.FromArgb(15, 23, 42),
            AutoSize = true,
            Margin = new Padding(0, 0, 0, 8)
        };
        titleBox.Controls.Add(titleLabel);

        var subtitleLabel = new Label
        {
            Text = "Ingresa tu correo institucional para vincular este equipo:",
            Font = new Font("Segoe UI", 11),
            ForeColor = Color.FromArgb(100, 116, 139),
            AutoSize = true,
            Margin = new Padding(0)
        };
        titleBox.Controls.Add(subtitleLabel);
        mainLayout.Controls.Add(titleBox);

        // Entrada.
        var inputRow = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            ColumnCount = 2,
            RowCount = 1,
            Margin = new Padding(0, 15, 0, 15)
        };
        inputRow.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        inputRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));

        var emailLabel = new Label
        {
            Text = "Correo:",
            Font = new Font("Segoe UI", 12, FontStyle.Bold),
            ForeColor = Color.FromArgb(15, 23, 42),
            AutoSize = true,
            Anchor = AnchorStyles.Left,
            TextAlign = ContentAlignment.MiddleLeft,
            Margin = new Padding(0, 0, 10, 0)
        };
        inputRow.Controls.Add(emailLabel, 0, 0);

        _emailInput = new TextBox
        {
            Font = new Font("Segoe UI", 12),
            Dock = DockStyle.Top,
            Margin = new Padding(0)
        };
        inputRow.Controls.Add(_emailInput, 1, 0);

        mainLayout.Controls.Add(inputRow);

        _statusLabel = new Label
        {
            Text = "",
            Font = new Font("Segoe UI", 10),
            AutoSize = true,
            Dock = DockStyle.Top,
            MinimumSize = new Size(0, 24),
            Margin = new Padding(0, 0, 0, 15)
        };
        mainLayout.Controls.Add(_statusLabel);

        // Botón.
        _verifyButton = new Button
        {
            Text = "Verificar y activar monitoreo",
            Font = new Font("Segoe UI", 11, FontStyle.Bold),
            BackColor = Color.FromArgb(220, 38, 38),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Height = 46,
            Dock = DockStyle.Top,
            Cursor = Cursors.Hand,
            Margin = new Padding(0, 0, 0, 25)
        };
        _verifyButton.FlatAppearance.BorderSize = 0;
        _verifyButton.Click += VerifyButton_Click;
        mainLayout.Controls.Add(_verifyButton);

        // Pie de página.
        var footerPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            ColumnCount = 1,
            RowCount = 3,
            Margin = new Padding(0, 0, 0, 5)
        };
        footerPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));

        var row1 = new Label
        {
            Text = "UAMOTORS | Departamento de Electronics",
            Font = new Font("Segoe UI", 9, FontStyle.Bold),
            ForeColor = Color.FromArgb(100, 116, 139),
            AutoSize = true,
            Anchor = AnchorStyles.None,
            Margin = new Padding(0, 0, 0, 4)
        };
        footerPanel.Controls.Add(row1, 0, 0);

        var row2 = new Label
        {
            Text = "Universidad Autónoma Metropolitana",
            Font = new Font("Segoe UI", 9),
            ForeColor = Color.FromArgb(100, 116, 139),
            AutoSize = true,
            Anchor = AnchorStyles.None,
            Margin = new Padding(0, 0, 0, 4)
        };
        footerPanel.Controls.Add(row2, 0, 1);

        var row3 = new LinkLabel
        {
            Text = "Desarrollado por Alejandro Ramírez",
            Font = new Font("Segoe UI", 9),
            LinkColor = Color.FromArgb(37, 99, 235),
            ActiveLinkColor = Color.FromArgb(37, 99, 235),
            AutoSize = true,
            Anchor = AnchorStyles.None,
            LinkBehavior = LinkBehavior.NeverUnderline,
            Margin = new Padding(0)
        };
        row3.LinkArea = new LinkArea(17, 17);
        row3.LinkClicked += (s, ev) => 
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo("https://github.com/lexrammart") { UseShellExecute = true });
        footerPanel.Controls.Add(row3, 0, 2);

        mainLayout.Controls.Add(footerPanel);

        // Versión.
        var versionLabel = new Label
        {
            Text = GetVersion(),
            Font = new Font("Segoe UI", 9),
            ForeColor = Color.FromArgb(148, 163, 184), // Color gris.
            AutoSize = true,
            Anchor = AnchorStyles.Right,
            Margin = new Padding(0, 10, 0, 0)
        };
        mainLayout.Controls.Add(versionLabel);

        this.AcceptButton = _verifyButton;
    }

    private async void VerifyButton_Click(object? sender, EventArgs e)
    {
        string email = _emailInput.Text;
        _statusLabel.Text = "Verificando en la base de datos de Drive...";
        _statusLabel.ForeColor = Color.FromArgb(37, 99, 235);
        _verifyButton.Enabled = false;

        var result = await Task.Run(() => UserService.VerifyUserEmail(email, _rutaUAMOTORS));

        if (result.Success)
        {
            UserService.SaveLocalProfile(email, result.Name!);
            _statusLabel.Text = $"✅ ¡Bienvenidx {result.Name}! Registro completado.";
            _statusLabel.ForeColor = Color.FromArgb(22, 163, 74);
            IsRegistered = true;
            await Task.Delay(3000);
            this.Close();
        }
        else
        {
            _statusLabel.Text = result.ErrorMsg;
            _statusLabel.ForeColor = Color.FromArgb(220, 38, 38);
            _verifyButton.Enabled = true;
            _emailInput.Enabled = true;
        }
    }
}
