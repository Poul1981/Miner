using System;
using System.Drawing;
using System.Windows.Forms;

namespace Miner
{
	internal class StateOfCell //модель через класс состояний
	{
		public bool Clear { get; set; } = true;
		public int Danger { get; set; } = 0;
		public bool Bomb { get; set; } = false;
		public bool Open { get; set; } = false;
		public bool Flag { get; set; } = false;
	}

	static class Program
	{
		static void Main()
		{
			var input = new InputPanel();
			Application.Run(input);
			int size = input.InputSize;
			if (size == 0) return;
			int bombs = input.Bombs;
			var game = new GameModel(size, bombs); //размер поля и кол-во бомб
			Application.Run(new BatleFild(game) { ClientSize = new Size(500, 500) });
		}
	}

	class GameModel
	{
		readonly StateOfCell[,] gameModel;
		public int Size { get; } //сделать ширину и длину
		public int Bombs { get; }
		public GameModel(int size, int bombs)//конструктор модели
		{
			Size = size;
			Bombs = bombs;
			gameModel = new StateOfCell[size, size];
			for (int i = 0; i < Size; i++)
				for (int j = 0; j < Size; j++)
				{
					gameModel[i, j] = new StateOfCell();
				}
			SetBombsPosition();
			SetDangerZones();
		}

		private void SetBombsPosition()//установить бомбы
		{
			int cell = Bombs;
			var rnd = new Random(156634);
			var rnd1 = new Random();
			if (Bombs > Size * Size) cell = Size * Size;
			for (int i = 0; i < cell; i++)
			{
				int x = rnd.Next(0, Size);
				int y = rnd1.Next(0, Size);
				if (gameModel[x, y].Bomb == true)
				{
					i--;
					continue;
				}
				else gameModel[x, y].Bomb = true;
			}
		}

		private int CheckField(int row, int col)
		{
			int bombs = 0;
			if (gameModel[row, col].Bomb == true) return 0;
			bool rSub = (row - 1) >= 0;
			bool cSub = (col - 1) >= 0;
			bool cUp = (col + 1) < Size;
			bool rUp = (row + 1) < Size;
			if (rSub && cSub && gameModel[row - 1, col - 1].Bomb) bombs++;
			if (rSub && gameModel[row - 1, col].Bomb) bombs++;
			if (rSub && cUp && gameModel[row - 1, col + 1].Bomb) bombs++;
			if (cSub && gameModel[row, col - 1].Bomb) bombs++;
			if (cUp && gameModel[row, col + 1].Bomb) bombs++;
			if (rUp && cSub && gameModel[row + 1, col - 1].Bomb) bombs++;
			if (rUp && gameModel[row + 1, col].Bomb) bombs++;
			if (rUp && cUp && gameModel[row + 1, col + 1].Bomb) bombs++;
			return bombs;
		}

		private void SetDangerZones()//пометить номерами опасные зоны
		{
			int bombs = 0;
			for (int i = 0; i < Size; i++)
				for (int j = 0; j < Size; j++)
					if (!gameModel[i, j].Bomb)
					{
						bombs = CheckField(i, j);
						gameModel[i, j].Danger = bombs;
					}
		}

		internal void DeepSearch(TableLayoutPanel table, int row, int column)
		{
			int danger = gameModel[row, column].Danger;
			Button btn = (Button)table.GetControlFromPosition(column, row);
			if (!gameModel[row, column].Open
				&& !gameModel[row, column].Flag
				&& gameModel[row, column].Danger == 0)
				Open(btn, row, column, table); //запускаем рекурсию
			else if (danger > 0 && gameModel[row, column].Flag == false)
			{
				gameModel[row, column].Open = true;
				DangerZone(btn, danger); //вызываем делегат (триггерим делегат)
			}
		}

		internal void Open(Button button, int row, int col, TableLayoutPanel table)
		{
			button.BackColor = Color.PaleGreen;
			//gameModel[row, col] = StateFild.Open;
			gameModel[row, col].Open = true;
			gameModel[row, col].Clear = false;
			bool rSub = (row - 1) >= 0;
			bool cSub = (col - 1) >= 0;
			bool cUp = (col + 1) < Size;
			bool rUp = (row + 1) < Size;

			if (rSub && cSub) DeepSearch(table, row - 1, col - 1);
			if (rSub) DeepSearch(table, row - 1, col);
			if (rSub && cUp) DeepSearch(table, row - 1, col + 1);
			if (cSub) DeepSearch(table, row, col - 1);
			if (cUp) DeepSearch(table, row, col + 1);
			if (rUp && cSub) DeepSearch(table, row + 1, col - 1);
			if (rUp) DeepSearch(table, row + 1, col);
			if (rUp && cUp) DeepSearch(table, row + 1, col + 1);
		}

		//////////обработка клика мыши//////////
		internal void ButtonMouseClick(TableLayoutPanel table, Button button, MouseEventArgs e)
		{
			int col = table.GetColumn(button);
			int row = table.GetRow(button);
			//StateFild cellState = gameModel[row, col];
			var cellState = gameModel[row, col];

			// ЛЕВЫЙ клик мыши
			if (e.Button == MouseButtons.Left && cellState.Flag == false)
			{
				//если опасная зона
				int danger = cellState.Danger;
				if (danger > 0)
				{
					cellState.Open = true;
					cellState.Clear = false;
					DangerZone?.Invoke(button, danger); // вызов делегата
				}
				//если попал на мину
				if (cellState.Bomb)
				{
					Exploid?.Invoke(table, gameModel); // вызов делегата
					cellState.Open = true;
					cellState.Clear = false;
				}
				//если поле чистое
				if (!cellState.Open && cellState.Clear)
					Open(button, row, col, table);

				//проверка выигрыша
				CheckVin();
			}
			//ПРАВЫЙ клик
			if (e.Button == MouseButtons.Right && !cellState.Open)
				if (cellState.Flag == false)
				{
					cellState.Flag = true;
					SetFlagChenged?.Invoke(button); // вызов делегата
				}
				else
				{
					cellState.Flag = false;
					RemuveFlagChenged?.Invoke(button); // вызов делегата
				}
		}

		private void CheckVin()
		{
			int openCell = 0;
			for (int i = 0; i < Size; i++)
				for (int j = 0; j < Size; j++)
				{
					if (gameModel[i, j].Open) openCell++;
				}
			if (openCell == Size * Size - Bombs)
			{
				var chek = MessageBox.Show("Играть заново?", "ПОБЕДА!!!",
				MessageBoxButtons.OKCancel);
				if (chek == DialogResult.Cancel) Application.Exit();
				if (chek == DialogResult.OK) Application.Restart();
			}
		}

		public event Action<Button> SetFlagChenged;// делегат для вызова метода смены флага
		public event Action<Button> RemuveFlagChenged;
		public event Action<Button, int> DangerZone; //делегат для установки опасной зоны
		public event Action<TableLayoutPanel, StateOfCell[,]> Exploid; //при взрыве
	}

	internal class FlagAttribute : Attribute
	{
		public bool Flag { get; set; }
		public FlagAttribute(bool flag)
		{ Flag = flag; }
	}

	class BatleFild : Form
	{
		readonly GameModel game;
		public TableLayoutPanel Table { get; }
		public static int MySize { get; set; }
		public BatleFild(GameModel mygame)//constructor поля
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(InputPanel));
			Icon = (Icon)(resources.GetObject("$this.Icon"));
			Text = "Сапер";
			game = mygame;
			MySize = game.Size;
			Table = new TableLayoutPanel();
			for (int i = 0; i < game.Size; i++) // строим сетку для кнопок размером сайзХсайз
			{
				Table.RowStyles.Add(new RowStyle(SizeType.Percent, 100f / game.Size));
				Table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / game.Size));
			}

			for (int column = 0; column < game.Size; column++) //заполняем сетку кнопками и ивентами
				for (int row = 0; row < game.Size; row++)
				{
					var button = new Button();
					button.Dock = DockStyle.Fill;
					button.BackColor = Color.DarkKhaki;
					button.MouseDown += (sender, arg) => game.ButtonMouseClick(Table, button, arg);
					Table.Controls.Add(button, column, row);
				}
			Table.Dock = DockStyle.Fill;
			Controls.Add(Table);
			game.RemuveFlagChenged += RemuveFlag;
			game.SetFlagChenged += SetFlag;
			game.DangerZone += ShowDanger;
			game.Exploid += Boom;
		}

		private void RemuveFlag(Button button)
		{
			button.BackgroundImage = null;
			button.BackColor = Color.DarkKhaki;
		}

		private void SetFlag(Button button)
		{
			button.BackgroundImage = Image.FromFile("flag.jpg");
			button.BackgroundImageLayout = ImageLayout.Stretch;
		}

		private void ShowDanger(Button button, int bombs)
		{
			button.Text = string.Format("{0}", bombs);
			button.BackColor = Color.PaleGreen;
		}

		private void Boom(TableLayoutPanel table, StateOfCell[,] model)
		{
			Button btn;
			for (int i = 0; i < MySize; i++)
				for (int j = 0; j < MySize; j++)
				{
					if (model[i, j].Bomb)
					{
						btn = (Button)table.GetControlFromPosition(j, i);
						btn.BackgroundImage = Image.FromFile("boom.jpg");
						btn.BackgroundImageLayout = ImageLayout.Stretch;
						//btn.Text = "BOOM";
						btn.BackColor = Color.Red;
					}
				}
			var chek = MessageBox.Show("Играть заново?", "ПРОИГРЫШ :(",
				MessageBoxButtons.OKCancel);
			if (chek == DialogResult.Cancel) Application.Exit();
			if (chek == DialogResult.OK) Application.Restart();
		}
	}
}
