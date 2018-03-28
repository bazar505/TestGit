using System;
using System.Windows.Forms;
using System.Threading.Tasks;

namespace TestGit
{
    public partial class Form1 : Form
    {
        public Form1() //.ctor 
        {
            InitializeComponent();
        }

        /** https://rsdn.org/forum/winapi/4159851.flat - интересная статья по системным таймерам **/

        private void Btn1_Click(object sender, EventArgs e) //Явно 
        {
            try
            {
                btn1.Enabled = false;
                btn2.Enabled = false;
                btn3.Enabled = false;

                lvTable.BeginUpdate(); //защита от мерцания контрола
                lvTable.Items.Clear();

                long Undo = 0, Diff = 0;
                TimeSpan ts;

                for (int i = 1; i < 26; i++)
                {
                    LoadProc(); //дали нагрузку на процессор для увеличения времени работы цикла
                    var d = DateTime.Now; //получили с чипа текущее время
                    var lv = lvTable.Items.Add(i.ToString()); //добавили строку в таблицу с номером итерации
                    lv.SubItems.Add(d.ToString("mm:ss.fffffff")); //2 колонка - время 
                    lv.SubItems.Add(d.Ticks.ToString("X")); //3 колонка - сериализовнное время
                    Diff = (Undo == 0) ? 0 : d.Ticks - Undo; //вычислили разницу между строками в тактах
                    ts = new TimeSpan(Diff);
                    lv.SubItems.Add($"{Diff.ToString()} ({ts.TotalMilliseconds} мс)"); //4 колонка - разница в тактах от прошлого значения
                    Undo = d.Ticks; //перезаполнили "прошлое" значение текущим для последующего сравнения
                    lv.BackColor = (Diff > 0) //фон строки белый для разницы 0, красный для 10000 и оранжевый для 100000
                        ? (Diff > 11000) ? System.Drawing.Color.Orange : System.Drawing.Color.MediumVioletRed
                        : System.Drawing.Color.White;
                }

                foreach (ColumnHeader c in lvTable.Columns) //делаем отображение таблицы удобным
                {
                    c.AutoResize(ColumnHeaderAutoResizeStyle.ColumnContent);
                    var w = c.Width;
                    c.AutoResize(ColumnHeaderAutoResizeStyle.HeaderSize);
                    if (c.Width < w) { c.Width = w; } //если столбец будет пуст, его ширина вернется к ширине по заголовку
                }
            }
            finally
            {
                lvTable.EndUpdate(); //обязательно вернули прорисовку обратно

                btn1.Enabled = true;
                btn2.Enabled = true;
                btn3.Enabled = true;
            }
        }

        private void Btn2_Click(object sender, EventArgs e) //В фоне 
        {
            btn1.Enabled = false;
            btn2.Enabled = false;
            btn3.Enabled = false;

            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            long Undo = 0, Diff = 0;
            TimeSpan ts;

            sb.Clear();
            Task t = Task.Run(() => //вся работы заключена в лямбде, которая работает в отдельном потоке через таск
            {
                int i = 1;
                while (i < 26)
                {
                    LoadProc(); //дали нагрузку на процессор для увеличения времени работы цикла
                    var d = DateTime.Now; //получили с чипа текущее время
                    Diff = (Undo == 0) ? 0 : d.Ticks - Undo; //вычислили разницу между строками в тактах
                    sb.AppendLine($"{i.ToString()}|{d.ToString("mm:ss.fffffff")}|{d.Ticks.ToString("X")}|{Diff}"); // код лямбды имеет доступ к локальным переменным вызывающей процедуры
                    Undo = d.Ticks; //перезаполнили "прошлое" значение текущим для последующего сравнения
                    i += 1;
                }
            });
            t.Wait();

            try //в качестве обратной связи от таска выбрал строку, которую нужно разделить на строки и столбцы. Существует масса более удобных вариантов, но хотел код посложнее для тренировки
            {
                lvTable.BeginUpdate(); //защита от мерцания контрола
                lvTable.Items.Clear();

                string[] L = sb.ToString().Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
                foreach (string NL in L)
                {
                    if (string.IsNullOrEmpty(NL)) { continue; }
                    string[] R = NL.Split(new[] { "|" }, StringSplitOptions.None);
                    var lv = lvTable.Items.Add(R[0]); //добавили строку в таблицу с номером итерации
                    lv.SubItems.Add(R[1]); //2 колонка - время 
                    lv.SubItems.Add(R[2]); //3 колонка - сериализовнное время
                    Diff = int.Parse(R[3]);
                    ts = new TimeSpan(Diff);
                    lv.SubItems.Add($"{R[3]} ({ts.TotalMilliseconds} мс)"); //4 колонка - разница в тактах от прошлого значения
                    lv.BackColor = (Diff > 0) //фон строки белый для разницы 0, красный для 10000 и оранжевый для 100000
                        ? (Diff > 11000) ? System.Drawing.Color.Orange : System.Drawing.Color.MediumVioletRed
                        : System.Drawing.Color.White;
                }

                foreach (ColumnHeader c in lvTable.Columns) //делаем отображение таблицы удобным
                {
                    c.AutoResize(ColumnHeaderAutoResizeStyle.ColumnContent);
                    var w = c.Width;
                    c.AutoResize(ColumnHeaderAutoResizeStyle.HeaderSize);
                    if (c.Width < w) { c.Width = w; } //если столбец будет пуст, его ширина вернется к ширине по заголовку
                }
            }
            finally
            {
                lvTable.EndUpdate(); //обязательно вернули прорисовку обратно

                btn1.Enabled = true;
                btn2.Enabled = true;
                btn3.Enabled = true;
            }
        }

        private void Btn3_Click(object sender, EventArgs e) //Слип 
        {
            try
            {
                btn1.Enabled = false;
                btn2.Enabled = false;
                btn3.Enabled = false;

                lvTable.BeginUpdate(); //защита от мерцания контрола
                lvTable.Items.Clear();

                long Undo = 0, Diff = 0;
                TimeSpan ts;

                for (int i = 1; i < 26; i++)
                {
                    System.Threading.Thread.Sleep(1); //дали прерывание кванта времени
                    var d = DateTime.Now; //получили с чипа текущее время
                    var lv = lvTable.Items.Add(i.ToString()); //добавили строку в таблицу с номером итерации
                    lv.SubItems.Add(d.ToString("mm:ss.fffffff")); //2 колонка - время 
                    lv.SubItems.Add(d.Ticks.ToString("X")); //3 колонка - сериализовнное время
                    Diff = (Undo == 0) ? 0 : d.Ticks - Undo; //вычислили разницу между строками в тактах
                    ts = new TimeSpan(Diff);
                    lv.SubItems.Add($"{Diff.ToString()} ({ts.TotalMilliseconds} мс)"); //4 колонка - разница в тактах от прошлого значения
                    Undo = d.Ticks; //перезаполнили "прошлое" значение текущим для последующего сравнения
                    lv.BackColor = (Diff > 0) //фон строки белый для разницы 0, красный для 10000 и оранжевый для 100000
                        ? (Diff > 11000) ? System.Drawing.Color.Orange : System.Drawing.Color.MediumVioletRed
                        : System.Drawing.Color.White;
                }

                foreach (ColumnHeader c in lvTable.Columns) //делаем отображение таблицы удобным
                {
                    c.AutoResize(ColumnHeaderAutoResizeStyle.ColumnContent);
                    var w = c.Width;
                    c.AutoResize(ColumnHeaderAutoResizeStyle.HeaderSize);
                    if (c.Width < w) { c.Width = w; } //если столбец будет пуст, его ширина вернется к ширине по заголовку
                }
            }
            finally
            {
                lvTable.EndUpdate(); //обязательно вернули прорисовку обратно
            }

            btn1.Enabled = true;
            btn2.Enabled = true;
            btn3.Enabled = true;
        }

        void LoadProc() //Бесполезная нагрузка 
        {
            for (int i = 0; i < 1000000; i++)
            { var y = i ^ 3; }
        }
    }
}
