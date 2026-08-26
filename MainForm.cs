using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Threading.Tasks;

namespace WhtsApi
{
    public class MainForm : Form
    {
        private PictureBox pbQrCode;
        private Label lblStatus;
        private Button btnReconnect;
        private Label lblApiStatus;
        private Timer sessionTimer;

        public MainForm()
        {
            InitializeComponent();
            this.Load += MainForm_Load;
        }

        private void InitializeComponent()
        {
            this.Text = "WhtsApi - WhatsApp Automation Client";
            this.Size = new Size(400, 450);
            this.StartPosition = FormStartPosition.CenterScreen;

            pbQrCode = new PictureBox
            {
                Location = new Point(50, 20),
                Size = new Size(280, 280),
                SizeMode = PictureBoxSizeMode.Zoom,
                BorderStyle = BorderStyle.FixedSingle
            };

            lblStatus = new Label
            {
                Location = new Point(50, 310),
                Size = new Size(280, 20),
                TextAlign = ContentAlignment.MiddleCenter,
                Text = "Status: Connecting to OpenWA..."
            };

            btnReconnect = new Button
            {
                Location = new Point(130, 340),
                Size = new Size(120, 30),
                Text = "Reconnect / Retry"
            };
            btnReconnect.Click += BtnReconnect_Click;

            lblApiStatus = new Label
            {
                Location = new Point(50, 380),
                Size = new Size(280, 20),
                TextAlign = ContentAlignment.MiddleCenter,
                Text = "Local API Server running on port 5000",
                ForeColor = Color.Green
            };

            this.Controls.Add(pbQrCode);
            this.Controls.Add(lblStatus);
            this.Controls.Add(btnReconnect);
            this.Controls.Add(lblApiStatus);

            sessionTimer = new Timer();
            sessionTimer.Interval = 5000;
            sessionTimer.Tick += SessionTimer_Tick;
        }

        private async void MainForm_Load(object sender, EventArgs e)
        {
            await LoadQrCode();
            sessionTimer.Start();
        }

        private async void BtnReconnect_Click(object sender, EventArgs e)
        {
            await LoadQrCode();
        }

        private async Task LoadQrCode()
        {
            lblStatus.Text = "Status: Fetching QR Code...";
            lblStatus.ForeColor = Color.Black;
            btnReconnect.Enabled = false;

            try
            {
                string base64Qr = await WhatsAppClient.Instance.GetQrCodeAsync();
                
                if (string.IsNullOrEmpty(base64Qr))
                {
                    lblStatus.Text = "Status: QR Code empty (maybe already connected?)";
                }
                else
                {
                    // If the string contains a data:image... prefix, remove it
                    if (base64Qr.Contains(","))
                    {
                        base64Qr = base64Qr.Split(',')[1];
                    }

                    byte[] imageBytes = Convert.FromBase64String(base64Qr);
                    using (var ms = new MemoryStream(imageBytes))
                    {
                        pbQrCode.Image = Image.FromStream(ms);
                    }
                    lblStatus.Text = "Status: Please scan QR Code";
                }
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Error: " + ex.Message;
                lblStatus.ForeColor = Color.Red;
            }
            finally
            {
                btnReconnect.Enabled = true;
            }
        }

        private async void SessionTimer_Tick(object sender, EventArgs e)
        {
            bool isConnected = await WhatsAppClient.Instance.CheckSessionAsync();
            if (isConnected)
            {
                lblStatus.Text = "Status: CONNECTED";
                lblStatus.ForeColor = Color.Green;
                pbQrCode.Image = null; // Clear QR code when connected
            }
            else
            {
                if (lblStatus.Text == "Status: CONNECTED")
                {
                    lblStatus.Text = "Status: DISCONNECTED";
                    lblStatus.ForeColor = Color.Red;
                }
            }
        }
    }
}
