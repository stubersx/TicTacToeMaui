using System;
using Microsoft.Maui.Controls;
using Plugin.Maui.Audio;

namespace TicTacToeMaui
{
    public partial class MainPage : ContentPage
    {
        private string currentPlayer;
        private string[,] board;
        private bool gameEnded;
        private IAudioPlayer _player1;
        private IAudioPlayer _player2;

        public MainPage()
        {
            InitializeComponent();
            StartNewGame();
        }

        private void StartNewGame()
        {
            currentPlayer = "🦃";
            board = new string[3, 3];
            gameEnded = false;
            statusLabel.Text = "Player 🦃's turn";
            ResetButtons();
            AnimateBoard();
        }
        
        private void ResetButtons()
        {
            btn00.Text = btn01.Text = btn02.Text = "";
            btn10.Text = btn11.Text = btn12.Text = "";
            btn20.Text = btn21.Text = btn22.Text = "";
            btn00.Background = btn02.Background = btn11.Background = btn20.Background = btn22.Background = Color.Parse("#EE8722");
            btn01.Background = btn10.Background = btn12.Background = btn21.Background = Color.Parse("Sienna");
            btn00.IsEnabled = btn01.IsEnabled = btn02.IsEnabled = true;
            btn10.IsEnabled = btn11.IsEnabled = btn12.IsEnabled = true;
            btn20.IsEnabled = btn21.IsEnabled = btn22.IsEnabled = true;
        }

        async void AnimateBoard()
        {
            await Task.WhenAll(
                Board.ScaleTo(0, 1000),
                Board.RotateTo(360, 1000),
                Board.FadeTo(0, 1000)
                );

            await Task.WhenAll(
                Board.ScaleTo(1, 1000),
                Board.RotateTo(0, 1000),
                Board.FadeTo(0.8, 1000)
                );
        }

        private async void OnButtonClicked(object sender, EventArgs e)
        {
            if (gameEnded) return;

            // Load the audio file from the app package
            if (_player1 == null)
            {
                var audioManager = AudioManager.Current;
                var audioFile = await FileSystem.OpenAppPackageFileAsync("bell.mp3");
                _player1 = audioManager.CreatePlayer(audioFile);
            }

            // Play the sound
            _player1.Play();

            var button = (Button)sender;
            int row = Grid.GetRow(button);
            int col = Grid.GetColumn(button);

            if (string.IsNullOrEmpty(board[row, col]))
            {
                board[row, col] = currentPlayer;
                button.Text = currentPlayer;

                if (button.Text == "🦃")
                    AnimateTurkey(button);
                else
                    AnimatePumpkin(button);

                if (CheckForWinner())
                {
                    if (_player2 == null)
                    {
                        var audioFile = await FileSystem.OpenAppPackageFileAsync("winning.mp3");
                        _player2 = AudioManager.Current.CreatePlayer(audioFile);
                    }

                    _player2.Play();
                    statusLabel.Text = $"Player {currentPlayer} wins!";
                    gameEnded = true;
                    DisableAllButtons();
                    AnimateStatus();
                }
                else if (CheckForDraw())
                {
                    statusLabel.Text = "It's a draw!";
                    gameEnded = true;
                }
                else
                {
                    SwitchPlayer();
                }
            }
        }

        private void AnimateTurkey(Button btn)
        {
            var BtnColor = new Animation(v => btn.Background = Color.FromRgba(0.4, 0.22 + v, 0.3 + v, 1), 0, 0.3);
            var SizeUp = new Animation(v => btn.FontSize = 40 + v * 20, 0, 1);
            var SizeDown = new Animation(v => btn.FontSize = 60 - v * 20, 0, 1);

            var BtnAnimation = new Animation();
            BtnAnimation.Add(0, 1, BtnColor);
            BtnAnimation.Add(0, 0.5, SizeUp);
            BtnAnimation.Add(0.5, 1, SizeDown);

            BtnAnimation.Commit(this, "ButtonAnimation", length: 1000, repeat: () => false);
        }

        private void AnimatePumpkin(Button btn)
        {
            var BtnColor = new Animation(v => btn.Background = Color.FromRgba(0 + v, 0, 0, 1), 0, 0.55);
            var SizeUp = new Animation(v => btn.FontSize = 40 + v * 20, 0, 1);
            var SizeDown = new Animation(v => btn.FontSize = 60 - v * 20, 0, 1);

            var BtnAnimation = new Animation();
            BtnAnimation.Add(0, 1, BtnColor);
            BtnAnimation.Add(0, 0.5, SizeUp);
            BtnAnimation.Add(0.5, 1, SizeDown);

            BtnAnimation.Commit(this, "ButtonAnimation", length: 1000, repeat: () => false);
        }

        private void AnimateStatus()
        {
            var BtR = new Animation(v => statusLabel.TextColor = Color.FromRgba(v, 0, 0, 1), 0, 1);
            var RtY = new Animation(v => statusLabel.TextColor = Color.FromRgba(1, v, 0, 1), 0, 1);
            var YtB = new Animation(v => statusLabel.TextColor = Color.FromRgba(1 - v, 1 - v, v, 1), 0, 1);
            var BtY = new Animation(v => statusLabel.TextColor = Color.FromRgba(0, 0 + v, 1, 1), 0, 1);
            var YtBl = new Animation(v => statusLabel.TextColor = Color.FromRgba(0, 1 - v, 1 - v, 1), 0, 1);

            var SizeUp = new Animation(v => statusLabel.FontSize = 35 + v * 10, 0, 1);
            var SizeDown = new Animation(v => statusLabel.FontSize = 45 - v * 10, 0, 1);

            var StatusAnimation = new Animation();
            StatusAnimation.Add(0, 0.2, BtR);
            StatusAnimation.Add(0.2, 0.4, RtY);
            StatusAnimation.Add(0.4, 0.6, YtB);
            StatusAnimation.Add(0.6, 0.8, BtY);
            StatusAnimation.Add(0.8, 1, YtBl);
            StatusAnimation.Add(0, 0.5, SizeUp);
            StatusAnimation.Add(0.5, 1, SizeDown);

            StatusAnimation.Commit(this, "StatusLabelAnimation", length: 3000, repeat: () => false);
        }

        private void SwitchPlayer()
        {
            currentPlayer = currentPlayer == "🦃" ? "🎃" : "🦃";
            statusLabel.Text = $"Player {currentPlayer}'s turn";
        }

        private bool CheckForWinner()
        {
            // Check rows, columns, and diagonals for a win
            for (int i = 0; i < 3; i++)
            {
                if (!string.IsNullOrEmpty(board[i, 0]) && board[i, 0] == board[i, 1] && board[i, 1] == board[i, 2])
                    return true;
                if (!string.IsNullOrEmpty(board[0, i]) && board[0, i] == board[1, i] && board[1, i] == board[2, i])
                    return true;
            }

            if (!string.IsNullOrEmpty(board[0, 0]) && board[0, 0] == board[1, 1] && board[1, 1] == board[2, 2])
                return true;
            if (!string.IsNullOrEmpty(board[0, 2]) && board[0, 2] == board[1, 1] && board[1, 1] == board[2, 0])
                return true;

            return false;
        }

        private bool CheckForDraw()
        {
            foreach (var cell in board)
            {
                if (string.IsNullOrEmpty(cell))
                    return false;
            }

            return true;
        }

        private void DisableAllButtons()
        {
            btn00.IsEnabled = btn01.IsEnabled = btn02.IsEnabled = false;
            btn10.IsEnabled = btn11.IsEnabled = btn12.IsEnabled = false;
            btn20.IsEnabled = btn21.IsEnabled = btn22.IsEnabled = false;
        }
        
        private void OnRestartClicked(object sender, EventArgs e)
        {
            StartNewGame();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            AnimateTitle();
        }

        async void AnimateTitle()
        {
            await Task.WhenAll(
                Title.FadeTo(0, 1000),
                Title.ScaleTo(0, 1000)
                );

            await Task.WhenAll(
                Title.FadeTo(1, 1000),
                Title.ScaleTo(1.5, 1000)
                );

            await Title.ScaleTo(1, 1000);
        }
    }
}
