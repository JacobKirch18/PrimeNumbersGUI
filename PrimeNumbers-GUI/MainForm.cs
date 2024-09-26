namespace PrimeNumbers_GUI
{
    public partial class MainForm : Form
    {

        private CancellationTokenSource? cancellationTokenSource;
        bool paused = false;
        int originalFirstNum;

        public MainForm()
        {
            InitializeComponent();
        }

        private async void StartCalculation(int firstNum, int lastNum)
        {
            // Prevent user from messing with certain controls while job is running
            progressBar1.Minimum = originalFirstNum;
            progressBar1.Maximum = lastNum;
            progressBar1.Visible = true;
            cancelButton.Enabled = true;
            pauseButton.Enabled = true;
            startNumTextBox.Enabled = false;
            endNumTextBox.Enabled = false;
            startButton.Enabled = false;

            UseWaitCursor = true;

            // See which numbers are factors and append them to the numbers text box
            await FindPrimes(firstNum, lastNum);

            // Let the user know we did something even if no prime nums were found
            if (numbersTextBox.TextLength == 0)
            {
                numbersTextBox.Text = "None.";
            }

            UseWaitCursor = false;

            // Reset the form
            if (!paused)
            {
                startNumTextBox.Enabled = true;
                endNumTextBox.Enabled = true;
                progressBar1.Value = progressBar1.Minimum;
                progressBar1.Visible = false;
                cancelButton.Enabled = false;
                pauseButton.Enabled = false;
                startButton.Enabled = true;
            }
        }

        private void startButton_Click(object sender, EventArgs e)
        {
            // Find all prime numbers starting between the first and last numbers
            int firstNum = 0;
            int lastNum = 0;
            pauseButton.Text = "Pause";

            if (!(int.TryParse(startNumTextBox.Text, out firstNum) && int.TryParse(endNumTextBox.Text, out lastNum)))
            {
                MessageBox.Show("Invalid input. Please enter a valid number.",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else if (firstNum >= lastNum)
            {
                MessageBox.Show("Invalid input. The first number must be less than the last number.",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                // Your existing code here
                // Find all prime numbers starting between the first and last numbers
                numbersTextBox.Clear();
                originalFirstNum = firstNum;
                StartCalculation(firstNum, lastNum);

            }
        }

        private async Task FindPrimes(int firstNum, int lastNum)
        {
            cancellationTokenSource = new CancellationTokenSource();
            var token = cancellationTokenSource.Token;

            await Task.Run(() =>
            {
                for (int i = firstNum; i <= lastNum; i++)
                {

                    if (token.IsCancellationRequested)
                    {
                        break;
                    }
                    if (IsPrime(i))
                    {
                        try
                        {
                            Invoke(() =>
                            {
                                AddNumberToTextBox(i);
                            });
                        }
                        catch (InvalidOperationException)
                        {
                            // The form was closed while we were working
                            break;
                        }
                    }
                }
            }, token);
        }

        private bool IsPrime(int num)
        {
            if (num < 2)
            {
                return false;
            }

            // Look for a number that evenly divides the num
            for (int i = 2; i <= num / 2; i++)
            {
                if (num % i == 0)
                {
                    return false;
                }
            }

            // No divisors means the number is prime
            return true;
        }

        private void AddNumberToTextBox(int num)
        {
            numbersTextBox.AppendText(num.ToString());
            numbersTextBox.AppendText(Environment.NewLine);
            progressBar1.Value = num;
        }

        private void pauseButton_Click(object sender, EventArgs e)
        {
            if (!paused)
            {
                paused = true;
                pauseButton.Text = "Resume";
                cancellationTokenSource?.Cancel();
            }
            else
            {
                paused = false;
                pauseButton.Text = "Pause";
                StartCalculation(progressBar1.Value, progressBar1.Maximum);
            }

        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            cancellationTokenSource?.Cancel();
        }
    }
}
