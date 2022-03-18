using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RatInAMaze
{
    public partial class F_RatInAMaze : Form
    {
        bool blnNaiveBT = true;
        bool blnNaiveBFS = false;
        int step = 0;
        int rowTarget = 2;
        int colTarget = 3;
        int rowTargetPrev = -1, colTargetPrev = -1;
        int[,] costs = new int[9, 9];
        int[,] board = new int[,]
        {
            { 4, 1, 0, 0, 0, 0, 0, 0, 0 },
            { 0, 1, 1, 0, 1, 0, 1, 1, 0 },
            { 0, 0, 1, 2, 1, 0, 1, 0, 0 },
            { 0, 0, 1, 1, 1, 0, 1, 0, 0 },
            { 1, 0, 0, 0, 0, 0, 1, 0, 0 },
            { 1, 1, 0, 0, 0, 0, 0, 1, 0 },
            { 0, 1, 0, 1, 1, 1, 0, 0, 0 },
            { 0, 1, 1, 1, 0, 0, 1, 0, 0 },
            { 0, 0, 0, 0, 0, 0, 0, 0, 3 },
        };

        List<CMove> Moves = new List<CMove>()
        {
            new CMove(){ Row = 1, Col = 0},
            new CMove(){ Row = 0, Col = 1},
            new CMove(){ Row =-1, Col = 0},
            new CMove(){ Row = 0, Col =-1},
        };

        public F_RatInAMaze()
        {
            InitializeComponent();

            generateBoard(board);
            setBoard(board);
        }

        private void generateBoard(int[,] board)
        {

            int rRow = new Random().Next(0, 9);
            int rCol = new Random().Next(0, 9);

            HashSet<int> RandList = new HashSet<int>();
            while (RandList.Count < 21)
            {
                int rVal = new Random().Next(0, 81);
                if (!RandList.Contains(rVal))
                    RandList.Add(rVal);
            }

            clearFootPrints(board);
            for (int r = 0; r < 9; r++)
            {
                for (int c = 0; c < 9; c++)
                {
                    if (RandList.Contains(r * 9 + c))
                        board[r, c] = 1;
                    else
                        board[r, c] = 0;
                }
            }


            rowTarget = (int)(RandList.Last() / 9.0);
            colTarget = RandList.Last() % 9;

            board[rowTarget, colTarget] = 2;
            board[0, 0] = 4;
            board[8, 8] = 3;

        }

        private void setBoard(int[,] boardData)
        {
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    Label lbl = tableLayoutPanel1.GetControlFromPosition(j, i) as Label;

                    if (boardData[i, j] > 0)
                    {
                        lbl.ImageList = imgList;
                        lbl.ImageIndex = boardData[i, j] - 1;
                    }
                    else
                        lbl.ImageList = null;

                }
            }
        }

        private async void btnSolver_ClickAsync(object sender, EventArgs e)
        {
            if (btnSolver.Text.Equals("Start"))
            {
                btnGenerate.Enabled = false;

                btnSolver.Text = "Stop";
                btnSolver.ImageList = imgList;
                btnSolver.ImageIndex = 5;

                step = 0;
                lblStep.Text = step.ToString();
                btnFinish.Enabled = true;
                clearFootPrints(board);
                board[0, 0] = 4;
                board[8, 8] = 3;
                try
                {
                    board[rowTargetPrev, colTargetPrev] = 2;
                    rowTarget = rowTargetPrev;
                    colTarget = colTargetPrev;
                }
                catch
                {
                    board[rowTarget, colTarget] = 2;
                }

                if (rbtnBFS.Checked)
                {
                    if (await SolveWithBFS(board))
                    {
                        MessageBox.Show("Solved.");
                    }
                    else
                    {
                        MessageBox.Show("No solution exists.");
                    }
                }
                else if (rbtnBFSwithCosts.Checked)
                {
                    if (await SolveWithBFSWithCosts(board))
                    {
                        MessageBox.Show("Solved.");
                    }
                    else
                    {
                        MessageBox.Show("No solution exists.");
                    }
                }
                else
                {
                    if (await SolveWithBT(board, 0, 0))
                    {
                        MessageBox.Show("Solved.");
                    }
                    else
                    {
                        MessageBox.Show("No solution exists.");
                    }
                }

                btnGenerate.Enabled = true;
            }
            else
            {
                btnSolver.Text = "Start";
                btnSolver.ImageList = null;
            }
        }


        private async Task<bool> SolveWithBFS(int[,] board)
        {
            Queue<CState> QStates = new Queue<CState>();
            CState initialState = new CState()
            {
                Grid = board,
                Row = 0,
                Col = 0
            };
            QStates.Enqueue(initialState);


            HashSet<string> hsExplored = new HashSet<string>();
            CState prev = initialState;
            while (QStates.Count > 0)
            {

                if (btnFinish.Enabled)
                {
                    await Task.Run(() =>
                    {
                        System.Threading.Thread.Sleep(100);
                    });
                }

                CState st = QStates.Dequeue();
                string code = st.ToString();
                hsExplored.Add(code);

                lblStep.Text = step.ToString();
                step++;


                if (st.Row == rowTarget && st.Col == colTarget)
                {

                    if (rowTarget == 8 && colTarget == 8)
                    {
                        // Final state

                        Form frm = new Form();
                        TreeView tv = new TreeView();
                        tv.Dock = DockStyle.Fill;
                        tv.AfterSelect += (sender, e) =>
                        {
                            CState selectedState = (tv.SelectedNode as CState);
                            setBoard(selectedState.Grid);
                        };

                        tv.Nodes.Add(initialState);
                        tv.SelectedNode = st;
                        frm.Controls.Add(tv);
                        frm.Show();

                        return true;
                    }
                    else
                    {
                        clearFootPrints(st.Grid);
                        QStates.Clear();

                        rowTargetPrev = rowTarget;
                        colTargetPrev = colTarget;

                        rowTarget = 8;
                        colTarget = 8;
                    }

                }


                foreach (var move in Moves)
                {
                    int rNew = move.Row + st.Row;
                    int cNew = move.Col + st.Col;


                    if (IsSafe(st.Grid, rNew, cNew))
                    {

                        int[,] boardNew = (int[,])st.Grid.Clone();

                        boardNew[rNew, cNew] = 4;
                        boardNew[st.Row, st.Col] = 5;

                        CState stNew = new CState()
                        {
                            Grid = boardNew,
                            Row = rNew,
                            Col = cNew
                        };

                        if (!hsExplored.Contains(stNew.ToString()))
                        {
                            st.Nodes.Add(stNew);
                            QStates.Enqueue(stNew);

                            setBoard(st.Grid);
                        }

                    }

                }


                prev = st;
            }



            return false;
        }


        private async Task<bool> SolveWithBFSWithCosts(int[,] board)
        {
            Queue<CState> QStates = new Queue<CState>();
            CState initialState = new CState()
            {
                Grid = board,
                Row = 0,
                Col = 0
            };
            QStates.Enqueue(initialState);
            initialState.UpdateCost(rowTarget, colTarget);


            HashSet<string> hsExplored = new HashSet<string>();
            CState prev = initialState;
            while (QStates.Count > 0)
            {

                if (btnFinish.Enabled)
                {
                    await Task.Run(() =>
                    {
                        System.Threading.Thread.Sleep(200);
                    });
                }

                CState st = QStates.Dequeue();
                string code = st.ToString();
                hsExplored.Add(code);


                lblStep.Text = step.ToString();
                step++;


                st.UpdateCost(rowTarget, colTarget);

                if (st.Cost == 0)
                {
                    if (rowTarget == 8 && colTarget == 8)
                    {

                        Form frm = new Form();
                        TreeView tv = new TreeView();
                        tv.Dock = DockStyle.Fill;
                        tv.AfterSelect += (sender, e) =>
                        {
                            CState selectedState = (tv.SelectedNode as CState);
                            setBoard(selectedState.Grid);
                        };

                        tv.Nodes.Add(initialState);
                        tv.SelectedNode = st;
                        frm.Controls.Add(tv);
                        frm.Show();

                        return true;
                    }
                    else
                    {
                        // clear foot-prints
                        clearFootPrints(st.Grid);
                        QStates.Clear();

                        rowTargetPrev = rowTarget;
                        colTargetPrev = colTarget;

                        rowTarget = 8;
                        colTarget = 8;

                        st.UpdateCost(rowTarget, colTarget);
                    }
                }



                foreach (var move in Moves)
                {
                    int rNew = move.Row + st.Row;
                    int cNew = move.Col + st.Col;


                    if (IsSafe(st.Grid, rNew, cNew))
                    {

                        int[,] boardNew = (int[,])st.Grid.Clone();

                        boardNew[rNew, cNew] = 4;
                        boardNew[st.Row, st.Col] = 5;

                        CState stNew = new CState()
                        {
                            Grid = boardNew,
                            Row = rNew,
                            Col = cNew
                        };

                        if (!hsExplored.Contains(stNew.ToString()))
                        {
                            st.Nodes.Add(stNew);

                            stNew.UpdateCost(rowTarget, colTarget);
                            QStates.Enqueue(stNew);

                            setBoard(st.Grid);
                        }

                    }

                }


                var list = QStates.OrderBy(s => s.Cost).ToList();
                QStates.Clear();
                foreach (var item in list)
                {
                    QStates.Enqueue(item);
                }


                prev = st;
            }



            return false;
        }

        private async Task<bool> SolveWithBT(int[,] board, int row, int col)
        {
            setBoard(board);

            lblStep.Text = step.ToString();
            step++;


            if (btnFinish.Enabled)
            {
                await Task.Run(() =>
                {
                    System.Threading.Thread.Sleep(200);
                });
            }



            costs = updateCosts(board, rowTarget, colTarget);
            if (costs[row, col] == 0)
            {
                if (rowTarget == 8 && colTarget == 8)
                {
                    return true;
                }
                else
                {
                    // clear foot-prints
                    clearFootPrints(board);
                    setBoard(board);


                    rowTargetPrev = rowTarget;
                    colTargetPrev = colTarget;

                    rowTarget = 8;
                    colTarget = 8;

                    costs = updateCosts(board, rowTarget, colTarget);
                }
            }

            if (blnNaiveBT)
            {
                return await NaiveBTAsync(board, row, col);
            }
            else
            {
                return await BTwithCostsAsync(board, row, col, costs);
            }

        }

        private async Task<bool> BTwithCostsAsync(int[,] board, int row, int col, int[,] costs)
        {
            Dictionary<CMove, int> moveVsCost = new Dictionary<CMove, int>();
            for (int i = 0; i < Moves.Count; i++)
            {
                int rNew = row + Moves[i].Row;
                int cNew = col + Moves[i].Col;
                if (IsSafe(board, rNew, cNew))
                    moveVsCost.Add(Moves[i], costs[rNew, cNew]);
            }

            foreach (var move in moveVsCost.OrderBy(d => d.Value).Select(d => d.Key))
            {
                int rNew = row + move.Row;
                int cNew = col + move.Col;

                if (IsSafe(board, rNew, cNew))
                {
                    board[row, col] = 5;
                    board[rNew, cNew] = 4;

                    if (await SolveWithBT(board, rNew, cNew))
                        return true;
                    else
                    {
                        board[row, col] = 4;
                        board[rNew, cNew] = 0;
                    }
                }

            }

            return false;
        }

        private async Task<bool> NaiveBTAsync(int[,] board, int row, int col)
        {

            foreach (var move in Moves)
            {
                int rNew = row + move.Row;
                int cNew = col + move.Col;

                if (IsSafe(board, rNew, cNew))
                {
                    board[row, col] = 5;
                    board[rNew, cNew] = 4;

                    if (await SolveWithBT(board, rNew, cNew))
                        return true;
                    else
                    {
                        board[row, col] = 4;
                        board[rNew, cNew] = 0;
                    }
                }

            }

            return false;
        }

        private void clearFootPrints(int[,] board)
        {
            for (int r = 0; r < 9; r++)
            {
                for (int c = 0; c < 9; c++)
                {
                    if (board[r, c] == 5)
                        board[r, c] = 0;
                }
            }
        }

        private bool IsSafe(int[,] board, int rNew, int cNew)
        {
            if (rNew < 0 || cNew < 0 || rNew > 8 || cNew > 8 || board[rNew, cNew] == 1 || board[rNew, cNew] == 5)
                return false;

            if (rowTarget != 8 || colTarget != 8)
                if (board[rNew, cNew] == 3)
                    return false;

            return true;
        }

        private int[,] updateCosts(int[,] board, int rowTarget, int colTarget)
        {
            int[,] weights = new int[9, 9];

            for (int r = 0; r < 9; r++)
            {
                for (int c = 0; c < 9; c++)
                {
                    weights[r, c] = Math.Abs(rowTarget - r) + Math.Abs(colTarget - c);

                    if (board[r, c] == 5)
                        weights[r, c] += 100;
                }
            }



            return weights;
        }

        public string ToOutput(int[,] arr, int limit)
        {
            StringBuilder sbRet = new StringBuilder();
            for (int i = 0; i < limit; i++)
            {
                for (int j = 0; j < limit; j++)
                {
                    sbRet.Append(arr[i, j].ToString().PadLeft(3, '0') + " ");
                }
                sbRet.AppendLine();
            }
            return sbRet.ToString();
        }

        private void rbtnNaiveBT_CheckedChanged(object sender, EventArgs e)
        {
            if (rbtnNaiveBT.Checked)
            {
                blnNaiveBT = true;
            }
        }

        private void rbtnBTwithCost_CheckedChanged(object sender, EventArgs e)
        {
            if (rbtnBTwithCost.Checked)
            {
                blnNaiveBT = false;
            }
        }

        private void btnFinish_Click(object sender, EventArgs e)
        {
            btnFinish.Enabled = false;
        }

        private void rbtnBFSwithCosts_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void rbtnBFS_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void btnGenerate_Click(object sender, EventArgs e)
        {
            generateBoard(board);
            setBoard(board);
        }
    }

    class CMove
    {
        public int Row { get; set; }
        public int Col { get; set; }

        public override string ToString()
        {
            return $"[{Row},{Col}]";
        }
    }

    class CState : TreeNode
    {
        public int[,] Grid { get; set; } = new int[9, 9];

        public int Row { get; set; }
        public int Col { get; set; }

        public bool IsFirstTargetAchieved { get; set; }
        public int Cost { get; set; }

        public override string ToString()
        {
            StringBuilder sbRet = new StringBuilder();
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    sbRet.Append(Grid[i, j].ToString() + " ");
                }
                sbRet.AppendLine();
            }
            this.Text = sbRet.ToString();
            return this.Text;
        }

        public void UpdateCost(int rowTarget, int colTarget)
        {
            Cost = (this.Row - rowTarget) * (this.Row - rowTarget) + (this.Col - colTarget) * (this.Col - colTarget);
        }
    }
}