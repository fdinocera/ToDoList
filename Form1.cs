using System.Data.SqlClient;

namespace ToDoList
{
    public partial class Form1 : Form
    {
        string connectionString = @"Server=DESKTOP-R7JUVDU\SQLEXPRESS03;Database=ToDoList;Trusted_Connection=True;Encrypt=False;";

        public Form1()
        {
            InitializeComponent();
            //CaricaAttivita();
        }

        void CaricaAttivita()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = "SELECT * FROM Tasks";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string idGenerato = reader["Id"].ToString();
                                string descrizione = reader["Description"].ToString();

                                DateTime scadenza = Convert.ToDateTime(reader["DueDate"]);

                                int priorita = Convert.ToInt32(reader["Priority"]);

                                bool completato = Convert.ToBoolean(reader["IsCompleted"]);

                                //carica record in listview
                                var item = new ListViewItem(idGenerato);
                                item.SubItems.Add(descrizione);
                                item.SubItems.Add(scadenza.ToShortDateString());
                                item.SubItems.Add(GetPriorita(priorita));
                                item.SubItems.Add(completato ? "Sì" : "No");
                                listView1.Items.Add(item);
                            }
                        }
                    }
                }
                catch (SqlException ex)
                {
                    MessageBox.Show(ex.Message);

                }
            }
        }

        void SalvaRecord()
        {
            string descrizione = textBox1.Text;
            DateTime scadenza = dateTimePicker1.Value;
            bool completato = checkBox1.Checked;
            int priorita = comboBox1.SelectedIndex;

            string query = @"INSERT INTO Tasks(Description, DueDate, IsCompleted, Priority) 
                                VALUES (@descrizione, @scadenza, @completato, @priorita)";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@descrizione", descrizione);
                        cmd.Parameters.AddWithValue("@scadenza", scadenza);
                        cmd.Parameters.AddWithValue("@completato", completato);
                        cmd.Parameters.AddWithValue("@priorita", priorita);

                        //recupera id generato da db
                        int idGenerato = Convert.ToInt32(cmd.ExecuteScalar());

                        //carica record in listview
                        var item = new ListViewItem(idGenerato.ToString());
                        item.SubItems.Add(descrizione);
                        item.SubItems.Add(scadenza.ToShortDateString());                        
                        item.SubItems.Add(GetPriorita(priorita));
                        item.SubItems.Add(completato ? "Sì" : "No");
                        listView1.Items.Add(item);
                    }
                }
                catch (SqlException ex)
                {
                    MessageBox.Show(ex.Message);

                }
            }
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            SalvaRecord();
        }

        private void Aggiorna_Click(object sender, EventArgs e)
        {
            AggiornaRecordCorrente();
        }

        private void Elimina_Click(object sender, EventArgs e)
        {
            EliminaRecordCorrente();
        }

        void EliminaRecordCorrente()
        {

            var selectedItem = listView1.SelectedItems[0];

            int record = int.Parse(selectedItem.Text);

            string query = @"DELETE FROM Tasks WHERE Id=@Id";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {

                try
                {
                    conn.Open();
                }
                catch (SqlException ex)
                {
                    MessageBox.Show(ex.Message);

                }

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@id", record);
                    cmd.ExecuteNonQuery();
                }


                listView1.Items.Remove(selectedItem);
            }

        }

        void AggiornaRecordCorrente()
        {
            int Id = int.Parse(listView1.SelectedItems[0].Text);

            string descrizione = textBox1.Text;
            DateTime scadenza = dateTimePicker1.Value;
            bool completato = checkBox1.Checked;
            int priorita = comboBox1.SelectedIndex;

            string query = @"UPDATE Tasks 
                               SET Description = @descrizione,
                               DueDate = @scadenza,
                               IsCompleted = @completato,
                               Priority = @priorita
                               WHERE Id = @Id";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                }
                catch (SqlException ex)
                {
                    MessageBox.Show(ex.Message);

                }

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@id", Id);
                    cmd.Parameters.AddWithValue("@descrizione", descrizione);
                    cmd.Parameters.AddWithValue("@scadenza", scadenza);
                    cmd.Parameters.AddWithValue("@completato", completato);
                    cmd.Parameters.AddWithValue("@priorita", priorita);
                    cmd.ExecuteNonQuery();

                    //carica record in listview
                    var item = listView1.SelectedItems[0];
                    item.SubItems[1].Text = descrizione;
                    item.SubItems[2].Text = scadenza.ToShortDateString();
                    item.SubItems[3].Text = GetPriorita(priorita);
                    item.SubItems[4].Text = completato ? "Sì" : "No";
                }
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            comboBox1.SelectedIndex = 0;
            comboBox2.SelectedIndex = 0;
        }

        private String GetPriorita(int p)
        {
            if (p == 0) return "Bassa";
            if (p == 1) return "Media";
            return "Alta";
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {

            var item = listView1.FocusedItem;
            textBox1.Text = item!.SubItems[1].Text;
            dateTimePicker1.Text = item.SubItems[2].Text;
            comboBox1.SelectedItem = item.SubItems[3].Text;
            checkBox1.Checked = item.SubItems[4].Text == "Sì" ? true : false;
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            // valore priorita
            var prioritaparam = comboBox2.SelectedIndex - 1;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    string query = "";
                    if (prioritaparam == -1)
                    {
                        query = "SELECT * FROM Tasks";
                    }
                    else
                    {
                        query = "SELECT * FROM Tasks WHERE Priority=@prioritaparam";
                    }                        

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        //assegnazione parametro
                        if (prioritaparam > -1)
                        {
                            cmd.Parameters.AddWithValue("@prioritaparam", prioritaparam);
                        }                            

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            listView1.Items.Clear();
                            while (reader.Read())
                            {
                                string idGenerato = reader["Id"].ToString();
                                string descrizione = reader["Description"].ToString();
                                DateTime scadenza = Convert.ToDateTime(reader["DueDate"]);
                                int priorita = Convert.ToInt32(reader["Priority"]);
                                bool completato = Convert.ToBoolean(reader["IsCompleted"]);

                                //carica record in listview                                
                                var item = new ListViewItem(idGenerato);
                                item.SubItems.Add(descrizione);
                                item.SubItems.Add(scadenza.ToShortDateString());
                                item.SubItems.Add(GetPriorita(priorita));
                                item.SubItems.Add(completato ? "Sì" : "No");
                                listView1.Items.Add(item);
                            }
                        }
                    }
                }
                catch (SqlException ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }
    }
}