using System;
using System.Drawing;
using System.Windows.Forms;

namespace Miner
{
    internal class InputPanel : Form
    {
        public int InputSize { get; set; }
        public int Bombs { get; set; }
        public InputPanel()
        {
            //Название и св-ва панели
            Text = "Сапер";
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(InputPanel));
            Icon = (Icon)(resources.GetObject("$this.Icon"));
            BackColor = Color.DarkKhaki;
            Load += (sender, args) => OnSizeChanged(EventArgs.Empty);

            ClientSize = new Size(350, 150);

            //элементы на форме
            var label2 = new Label();//введите кол-во клеток
            var label3 = new Label();// введите колво бомб
            var box1 = new TextBox(); // ввод кол-ва клеток
            var box2 = new TextBox(); // коэф бомб
            var button = new Button();// начали

            //расположение
            SizeChanged += (sender, args) =>
            {
                //введите кол-во клеток
                label2.Location = new Point(10, 10);
                label2.AutoSize = true;

                // введите колво бомб
                label3.Location = new Point(10, label2.Bottom + 10);
                label3.AutoSize = true;

                box1.Location = new Point(label2.Right + 50, label2.Top);
                box1.Size = new Size(50, 10);

                box2.Location = new Point(box1.Left, label3.Top);
                box2.Size = new Size(50, 10);

                button.Location = new Point(ClientSize.Width / 3, label3.Bottom + 10);
                button.Size = new Size(105, 40);
            };

            label2.Text = "Введите размер поля";
            label2.Font = new Font("Arial", 12);
            Controls.Add(label2);

            label3.Text = "Введите количество бомб";
            label3.Font = new Font("Arial", 12);
            Controls.Add(label3);

            box1.Text = "10";
            box2.Text = "8";
            Controls.Add(box1);
            Controls.Add(box2);

            button.Text = "Поехали!";
            button.Font = new Font("Arial", 12);
            Controls.Add(button);

            button.Click += (sender, args) =>
            {
                bool flag = true;
                try
                {
                    InputSize = int.Parse(box1.Text);
                    if (InputSize < 3 || InputSize > 20)
                    {
                        MessageBox.Show("Количество клеток поля должно быть от 3 до 20");
                        flag = false;
                    }
                }
                catch
                {
                    MessageBox.Show("Ошибочный ввод! Введите количество клеток!");
                    flag = false;
                }

                try
                {
                    Bombs = int.Parse(box2.Text);
                    if (Bombs <= 1 || Bombs >= 21)
                    {
                        MessageBox.Show("Количество бомб должно быть от 1 до 20.");
                        flag = false;
                    }
                }
                catch
                {
                    MessageBox.Show("Ошибочный ввод! Введите количество бомб!");
                    flag = false;
                }
                if (flag) Close();
            };
        }

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(InputPanel));
            this.SuspendLayout();
            // 
            // InputPanel
            // 
            this.ClientSize = new System.Drawing.Size(282, 255);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "InputPanel";
            this.ResumeLayout(false);

        }
    }
}
