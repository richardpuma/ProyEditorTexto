using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EditorTexto
{
    public partial class FormPrincipal : Form
    {
        private int ContadorContenedor = 0;

        public FormPrincipal()
        {
            InitializeComponent();
        }

        private void FormPrincipal_Load(object sender, EventArgs e)
        {
            AgregarNuevoDocumento();
            ColeccionTipoLetra();
            TamanoLetras();
        }
        #region Funcionalidades del formulario
        private int tolerance = 12;
        private const int WM_NCHITTEST = 132;
        private const int HTBOTTOMRIGHT = 17;
        private Rectangle sizeGripRectangle;

        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case WM_NCHITTEST:
                    base.WndProc(ref m);
                    var hitPoint = this.PointToClient(new Point(m.LParam.ToInt32() & 0xffff, m.LParam.ToInt32() >> 16));
                    if (sizeGripRectangle.Contains(hitPoint))
                        m.Result = new IntPtr(HTBOTTOMRIGHT);
                    break;
                default:
                    base.WndProc(ref m);
                    break;
            }
        }
        //----------------DIBUJAR RECTANGULO / EXCLUIR ESQUINA PANEL 
        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            var region = new Region(new Rectangle(0, 0, this.ClientRectangle.Width, this.ClientRectangle.Height));

            sizeGripRectangle = new Rectangle(this.ClientRectangle.Width - tolerance, this.ClientRectangle.Height - tolerance, tolerance, tolerance);

            region.Exclude(sizeGripRectangle);
            this.panelContenedor.Region = region;
            this.Invalidate();
        }
        //----------------COLOR Y GRIP DE RECTANGULO INFERIOR
        protected override void OnPaint(PaintEventArgs e)
        {
            SolidBrush blueBrush = new SolidBrush(Color.FromArgb(244, 244, 244));
            e.Graphics.FillRectangle(blueBrush, sizeGripRectangle);

            base.OnPaint(e);
            ControlPaint.DrawSizeGrip(e.Graphics, Color.Transparent, sizeGripRectangle);
        }

        private void panelBarraTitulo_MouseMove(object sender, MouseEventArgs e)
        {
            ReleaseCapture();
            SendMessage(this.Handle, 0x112, 0xf012, 0);
        }

        //METODO PARA ARRASTRAR EL FORMULARIO---------------------------------------------------------------------
        [DllImport("user32.DLL", EntryPoint = "ReleaseCapture")]
        private extern static void ReleaseCapture();

        [DllImport("user32.DLL", EntryPoint = "SendMessage")]
        private extern static void SendMessage(System.IntPtr hWnd, int wMsg, int wParam, int lParam);

        private void btnSalir_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("¿Esta seguro de salir del Editor de Texto?", "Alerta!", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1) == DialogResult.Yes)
            {
                Application.Exit();
            }
        }

        private void btnMinimizar_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        //Capturar posicion y tamaño antes de maximizar para restaurar
        int lx, ly;
        int sw, sh;
        private void btnMaximizar_Click(object sender, EventArgs e)
        {
            lx = this.Location.X;
            ly = this.Location.Y;
            sw = this.Size.Width;
            sh = this.Size.Height;
            //btnMaximizar.Visible = false;
            //btnRestaurar.Visible = true;
            this.Size = Screen.PrimaryScreen.WorkingArea.Size;
            this.Location = Screen.PrimaryScreen.WorkingArea.Location;
        }

        private void btnRestaurar_Click(object sender, EventArgs e)
        {
            //btnMaximizar.Visible = true;
            //btnRestaurar.Visible = false;
            this.Size = new Size(sw, sh);
            this.Location = new Point(lx, ly);
        }

        private void btnCerrar_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("¿Esta seguro de salir del Editor de Texto?", "Alerta!", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1) == DialogResult.Yes)
            {
                Application.Exit();
            }
        }
        #endregion

        #region Métodos

        #region Documentos

        private void AbrirDocumento()
        {
            abrirDocumento.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            abrirDocumento.Filter = "RTF|*.rtf|Text Files|*.txt|VB Files|*.vb|C# Files|*.cs|All Files|*.*";

            if (abrirDocumento.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if (abrirDocumento.FileName.Length > 9)
                {
                    ObtenerDocumentoActual.LoadFile(abrirDocumento.FileName, RichTextBoxStreamType.TextTextOleObjs);
                }
            }

        }

        private void AgregarNuevoDocumento()
        {

            RichTextBox Body = new RichTextBox();

            Body.Name = "Body";
            Body.Dock = DockStyle.Fill;
            Body.ContextMenuStrip = MenuContexto;

            TabPage NuevaPagina = new TabPage();
            ContadorContenedor += 1;

            string DocumentText = "Documento " + ContadorContenedor;
            NuevaPagina.Name = DocumentText;
            NuevaPagina.Text = DocumentText;
            NuevaPagina.Controls.Add(Body);

            contenedorDocumento.TabPages.Add(NuevaPagina);

        }

        private void EliminarDocumento()
        {
            if (contenedorDocumento.TabPages.Count != 1)
            {
                contenedorDocumento.TabPages.Remove(contenedorDocumento.SelectedTab);
            }
            else
            {
                contenedorDocumento.TabPages.Remove(contenedorDocumento.SelectedTab);
                AgregarNuevoDocumento();
            }
        }

        #endregion

        #region General

        private void ColeccionTipoLetra()
        {
            InstalledFontCollection InsFonts = new InstalledFontCollection();

            foreach (FontFamily item in InsFonts.Families)
            {
                cboFuentes.Items.Add(item.Name);
            }
            cboFuentes.SelectedIndex = 0;
        }

        private void TamanoLetras()
        {
            for (int i = 1; i <= 75; i++)
            {
                cboTamanoTexto.Items.Add(i);
            }

            cboTamanoTexto.SelectedIndex = 11;
        }


        private void cboFuentes_SelectedIndexChanged(object sender, EventArgs e)
        {
            Font NuevoFontTexto = new Font(cboFuentes.SelectedItem.ToString(), ObtenerDocumentoActual.SelectionFont.Size, ObtenerDocumentoActual.SelectionFont.Style);

            ObtenerDocumentoActual.SelectionFont = NuevoFontTexto;
        }

        private void cboTamanoTexto_SelectedIndexChanged(object sender, EventArgs e)
        {
            float NuevoTamano;

            float.TryParse(cboTamanoTexto.SelectedItem.ToString(), out NuevoTamano);

            Font NuevoFontTexto = new Font(ObtenerDocumentoActual.SelectionFont.Name, NuevoTamano, ObtenerDocumentoActual.SelectionFont.Style);

            ObtenerDocumentoActual.SelectionFont = NuevoFontTexto;
        }


        #endregion

        #endregion

        #region Propiedades

        private RichTextBox ObtenerDocumentoActual
        {
            get { return (RichTextBox)contenedorDocumento.SelectedTab.Controls["Body"]; }
        }

        #endregion

        #region Eventos



        #region MenuContexto

        private void btnNuevo_Click(object sender, EventArgs e)
        {
            AgregarNuevoDocumento();
        }

        private void btnEliminar_Click(object sender, EventArgs e)
        {
            EliminarDocumento();
        }

        private void btnAbrir_Click(object sender, EventArgs e)
        {
            AbrirDocumento();
        }

        private void menuNuevo_Click(object sender, EventArgs e)
        {
            AgregarNuevoDocumento();
        }

        private void menuAbrir_Click(object sender, EventArgs e)
        {
            AbrirDocumento();
        }

        private void menuSalir_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("¿Esta seguro de salir del Editor de Texto?", "Alerta!", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1) == DialogResult.Yes)
            {
                Application.Exit();
            }
        }

        private void btnColorTexto_Click(object sender, EventArgs e)
        {
            if (colorOpcion.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                ObtenerDocumentoActual.SelectionColor = colorOpcion.Color;
            }
        }

        private void btnColorFondo_Click(object sender, EventArgs e)
        {

        }

        #endregion

        #endregion
    }
}
