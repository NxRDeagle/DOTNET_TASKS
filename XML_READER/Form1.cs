using System;
using System.Windows.Forms;
using System.Xml;

namespace XML_READER
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        /** Обработчик нажатия на кнопку */
        private void button1_Click(object sender, EventArgs e)
        {
            // Файл с входными данными data.XML должен находиться в директории проекта.
            string path = "data.xml";
            // Загружаем xml
            addItemsToListBox(path);
        }

        /** Обработчик нажатия по боксу */
        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                // Выбор XML файла для загрузки.
                OpenFileDialog dialog = new OpenFileDialog();
                dialog.Filter = "XML Files|*.xml";
                dialog.Title = "Выберите файл в формате XML";

                // Если нажали ОК то загружаем XML.
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    string path = dialog.FileName;
                    addItemsToListBox(path);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }

        /** Функция добавления человечков в listBox */
        private void addItemsToListBox(string path)
        {
            try
            {
                // Очистка списка перед загрузкой.
                listBox1.Items.Clear();
                XmlDocument doc = new XmlDocument();
                doc.Load(path);
                // Считываем в цикле все узлы Person.
                XmlNodeList personNodes = doc.SelectNodes("//Person");
                foreach (XmlNode node in personNodes)
                {
                    string name = node.SelectSingleNode("Name")?.InnerText;
                    string surname = node.SelectSingleNode("Surname")?.InnerText;
                    string age = node.SelectSingleNode("Age")?.InnerText;
                    listBox1.Items.Add($"{name} {surname}, {age} лет");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }
    }
}
