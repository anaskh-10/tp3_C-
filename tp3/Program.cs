using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

namespace TP3
{
        public class Compte
    {
        private string _numero;
        private string _titulaire;
        private decimal _solde;

        public Compte(string numero, string titulaire, decimal soldeInitial)
        {
            if (string.IsNullOrWhiteSpace(numero)) throw new ArgumentException("Le numéro de compte est obligatoire.");
            if (string.IsNullOrWhiteSpace(titulaire)) throw new ArgumentException("Le nom du titulaire est obligatoire.");
            if (soldeInitial < 0) throw new ArgumentException("Le solde initial ne peut pas être négatif.");
            _numero = numero.Trim();
            _titulaire = titulaire.Trim();
            _solde = soldeInitial;
        }

        public string Numero
        {
            get { return _numero; }
            set
            {
                if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException("Numéro invalide.");
                _numero = value.Trim();
            }
        }
        public string Titulaire
        {
            get { return _titulaire; }
            set
            {
                if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException("Titulaire invalide.");
                _titulaire = value.Trim();
            }
        }
        public decimal Solde
        {
            get { return _solde; }
            protected set { _solde = value; }
        }

        public virtual void Deposer(decimal montant)
        {
            if (montant <= 0) throw new ArgumentException("Le montant doit être strictement positif.");
            _solde += montant;
        }

        public virtual bool Retirer(decimal montant)
        {
            if (montant <= 0) throw new ArgumentException("Le montant doit être strictement positif.");
            if (montant > _solde) return false;
            _solde -= montant;
            return true;
        }

        public virtual string Afficher()
        {
            return "Compte N° " + _numero + " | Titulaire : " + _titulaire + " | Solde : " + _solde.ToString("0.000") + " DT";
        }
    }

        public class CompteCourant : Compte
    {
        private decimal _frais;
        public decimal Frais { get { return _frais; } set { if (value < 0) throw new ArgumentException("Frais invalides."); _frais = value; } }

        public CompteCourant(string numero, string titulaire, decimal soldeInitial, decimal frais = 1m)
            : base(numero, titulaire, soldeInitial) { Frais = frais; }

        public override void Deposer(decimal montant)
        {
            if (montant <= _frais) throw new ArgumentException("Le dépôt doit dépasser les frais d'opération (" + _frais.ToString("0.000") + " DT).");
            base.Deposer(montant - _frais);
        }
        public override bool Retirer(decimal montant)
        {
            if (montant <= 0) throw new ArgumentException("Le montant doit être strictement positif.");
            return base.Retirer(montant + _frais);          // le solde doit couvrir montant + frais
        }
        public override string Afficher() { return "[Courant] " + base.Afficher() + " | Frais/opération : " + _frais.ToString("0.000") + " DT"; }
    }

    public class CompteEpargne : Compte
    {
        private decimal _taux;      // ex : 0.05 = 5 %
        public decimal Taux { get { return _taux; } set { if (value < 0) throw new ArgumentException("Taux invalide."); _taux = value; } }

        public CompteEpargne(string numero, string titulaire, decimal soldeInitial, decimal taux = 0.05m)
            : base(numero, titulaire, soldeInitial) { Taux = taux; }

        public decimal AjouterInterets()
        {
            decimal interets = Solde * _taux;
            Solde += interets;
            return interets;
        }
        public override string Afficher() { return "[Epargne] " + base.Afficher() + " | Taux : " + (_taux * 100).ToString("0.##") + " %"; }
    }

       static class Util
    {
        public static bool TryDec(string s, out decimal v)
        {
            return decimal.TryParse(s.Trim().Replace(',', '.'), NumberStyles.Number, CultureInfo.InvariantCulture, out v);
        }
        public static Label Lbl(Control p, string t, int x, int y, int w = 140, int h = 22, bool bold = false)
        {
            var l = new Label { Text = t, Left = x, Top = y, Width = w, Height = h, Font = new Font("Segoe UI", 9f, bold ? FontStyle.Bold : FontStyle.Regular) };
            p.Controls.Add(l); return l;
        }
        public static TextBox Txt(Control p, string name, int x, int y, int w = 160)
        { var t = new TextBox { Name = name, Left = x, Top = y, Width = w }; p.Controls.Add(t); return t; }
        public static Button Btn(Control p, string name, string t, int x, int y, int w = 110, int h = 30)
        { var b = new Button { Name = name, Text = t, Left = x, Top = y, Width = w, Height = h }; p.Controls.Add(b); return b; }
    }

    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.Run(new frmCompte());
        }
    }

       class frmCompte : Form
    {
        TextBox txtNumero, txtTitulaire, txtSoldeInitial, txtMontant;
        Button btnCreer, btnDeposer, btnRetirer;
        Label lblAffichage;
        Compte compte;      // le compte courant (null tant que non cree)

        public frmCompte()
        {
            Text = "Gestion d'un compte bancaire"; ClientSize = new Size(520, 380);
            StartPosition = FormStartPosition.CenterScreen;

            Util.Lbl(this, "Numéro de compte :", 20, 22); txtNumero = Util.Txt(this, "txtNumero", 170, 19);
            Util.Lbl(this, "Titulaire :", 20, 55); txtTitulaire = Util.Txt(this, "txtTitulaire", 170, 52);
            Util.Lbl(this, "Solde initial :", 20, 88); txtSoldeInitial = Util.Txt(this, "txtSoldeInitial", 170, 85);
            btnCreer = Util.Btn(this, "btnCreer", "Créer le compte", 350, 50, 140, 34);

            Util.Lbl(this, "Montant :", 20, 150); txtMontant = Util.Txt(this, "txtMontant", 170, 147);
            btnDeposer = Util.Btn(this, "btnDeposer", "Déposer", 20, 190, 130, 34);
            btnRetirer = Util.Btn(this, "btnRetirer", "Retirer", 170, 190, 130, 34);
            var btnP2 = Util.Btn(this, "btnPartie2", "Partie 2 (héritage)", 350, 190, 140, 34);

            lblAffichage = Util.Lbl(this, "Aucun compte créé.", 20, 250, 470, 90);
            lblAffichage.BorderStyle = BorderStyle.FixedSingle; lblAffichage.Padding = new Padding(6);

            InitialiserFormulaire();
            btnCreer.Click += btnCreer_Click;
            btnDeposer.Click += btnDeposer_Click;
            btnRetirer.Click += btnRetirer_Click;
            btnP2.Click += (s, e) => new frmTypeCompte().ShowDialog();
        }

        private void InitialiserFormulaire()
        {
            compte = null;
            txtNumero.Clear(); txtTitulaire.Clear(); txtSoldeInitial.Text = "0"; txtMontant.Clear();
            txtNumero.Enabled = txtTitulaire.Enabled = txtSoldeInitial.Enabled = btnCreer.Enabled = true;
            txtMontant.Enabled = btnDeposer.Enabled = btnRetirer.Enabled = false;
            lblAffichage.Text = "Aucun compte créé."; lblAffichage.ForeColor = Color.Black;
            txtNumero.Focus();
        }

        private void btnCreer_Click(object sender, EventArgs e)
        {
            if (!Util.TryDec(txtSoldeInitial.Text, out decimal solde))
            {
                MessageBox.Show("Solde initial invalide.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtSoldeInitial.Focus(); return;
            }
            try
            {
                compte = new Compte(txtNumero.Text, txtTitulaire.Text, solde);
                txtNumero.Enabled = txtTitulaire.Enabled = txtSoldeInitial.Enabled = btnCreer.Enabled = false;
                txtMontant.Enabled = btnDeposer.Enabled = btnRetirer.Enabled = true;
                Afficher("Compte créé avec succès.", Color.Green);
                txtMontant.Focus();
            }
            catch (ArgumentException ex)
            {
                MessageBox.Show(ex.Message, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnDeposer_Click(object sender, EventArgs e)
        {
            if (!MontantValide(out decimal m)) return;
            try { compte.Deposer(m); Afficher("Dépôt de " + m.ToString("0.000") + " DT effectué.", Color.Green); }
            catch (ArgumentException ex) { Afficher(ex.Message, Color.Red); }
            txtMontant.Clear(); txtMontant.Focus();
        }

        private void btnRetirer_Click(object sender, EventArgs e)
        {
            if (!MontantValide(out decimal m)) return;
            try
            {
                if (compte.Retirer(m)) Afficher("Retrait de " + m.ToString("0.000") + " DT effectué.", Color.Green);
                else Afficher("Retrait refusé : solde insuffisant.", Color.Red);
            }
            catch (ArgumentException ex) { Afficher(ex.Message, Color.Red); }
            txtMontant.Clear(); txtMontant.Focus();
        }

        private bool MontantValide(out decimal m)
        {
            if (!Util.TryDec(txtMontant.Text, out m) || m <= 0)
            {
                MessageBox.Show("Saisir un montant strictement positif.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtMontant.Focus(); return false;
            }
            return true;
        }

        private void Afficher(string message, Color c)
        {
            lblAffichage.ForeColor = c;
            lblAffichage.Text = message + Environment.NewLine + Environment.NewLine + compte.Afficher();
        }
    }

       class frmTypeCompte : Form
    {
        ComboBox cmbTypeCompte; TextBox txtMontant;
        Button btnDeposer, btnRetirer, btnInteret;
        Label lblTitulaire, lblSolde, lblLogs;
        Compte courant = new CompteCourant("C-001", "Titulaire démo", 100m, 1m);
        Compte epargne = new CompteEpargne("E-001", "Titulaire démo", 100m, 0.05m);
        Compte actif;

        public frmTypeCompte()
        {
            Text = "Types de compte"; ClientSize = new Size(520, 470);
            StartPosition = FormStartPosition.CenterParent;

            Util.Lbl(this, "Type de compte :", 20, 22);
            cmbTypeCompte = new ComboBox { Name = "cmbTypeCompte", Left = 170, Top = 19, Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbTypeCompte.Items.AddRange(new object[] { "Compte Courant", "Compte Épargne" });
            Controls.Add(cmbTypeCompte);

            Util.Lbl(this, "Montant :", 20, 60); txtMontant = Util.Txt(this, "txtMontant", 170, 57);
            btnDeposer = Util.Btn(this, "btnDeposer", "Déposer", 20, 100, 110);
            btnRetirer = Util.Btn(this, "btnRetirer", "Retirer", 145, 100, 110);
            btnInteret = Util.Btn(this, "btnInteret", "Ajouter intérêts", 270, 100, 130);

            lblTitulaire = Util.Lbl(this, "", 20, 150, 470, 24, true);
            lblSolde = Util.Lbl(this, "", 20, 178, 470, 24, true);
            lblLogs = Util.Lbl(this, "", 20, 215, 470, 230);
            lblLogs.BorderStyle = BorderStyle.FixedSingle; lblLogs.Padding = new Padding(6); lblLogs.BackColor = Color.White;

            cmbTypeCompte.SelectedIndexChanged += (s, e) => ChangerType();
            btnDeposer.Click += btnDeposer_Click; btnRetirer.Click += btnRetirer_Click; btnInteret.Click += btnInteret_Click;
            cmbTypeCompte.SelectedIndex = 0;
        }

        void ChangerType()
        {
            actif = cmbTypeCompte.SelectedIndex == 0 ? courant : epargne;
            btnInteret.Enabled = actif is CompteEpargne;        // actif uniquement pour le compte epargne
            Log("--- " + cmbTypeCompte.Text + " sélectionné ---");
            MajAffichage();
        }

        void MajAffichage()
        {
            lblTitulaire.Text = "Titulaire : " + actif.Titulaire + "   (N° " + actif.Numero + ")";
            lblSolde.Text = "Solde : " + actif.Solde.ToString("0.000") + " DT";
        }

        void Log(string s) { lblLogs.Text = s + Environment.NewLine + lblLogs.Text; }

        bool Montant(out decimal m)
        {
            if (!Util.TryDec(txtMontant.Text, out m) || m <= 0)
            { MessageBox.Show("Saisir un montant strictement positif.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning); return false; }
            return true;
        }

        void btnDeposer_Click(object sender, EventArgs e)
        {
            if (!Montant(out decimal m)) return;
            try { actif.Deposer(m); Log("Dépôt de " + m.ToString("0.000") + " DT"); }
            catch (ArgumentException ex) { Log("Refusé : " + ex.Message); }
            MajAffichage(); txtMontant.Clear();
        }

        void btnRetirer_Click(object sender, EventArgs e)
        {
            if (!Montant(out decimal m)) return;
            try { Log(actif.Retirer(m) ? "Retrait de " + m.ToString("0.000") + " DT" : "Retrait refusé : solde insuffisant"); }
            catch (ArgumentException ex) { Log("Refusé : " + ex.Message); }
            MajAffichage(); txtMontant.Clear();
        }

        void btnInteret_Click(object sender, EventArgs e)
        {
            var ce = actif as CompteEpargne;
            if (ce == null) return;
            decimal i = ce.AjouterInterets();
            Log("Intérêts ajoutés : " + i.ToString("0.000") + " DT (taux " + (ce.Taux * 100).ToString("0.##") + " %)");
            MajAffichage();
        }
    }
}