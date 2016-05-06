namespace Overstat
{
  partial class Setting
  {
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
      if (disposing && (components != null))
      {
        components.Dispose();
      }
      base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Setting));
      this.startAuthButton = new System.Windows.Forms.Button();
      this.pinInputBox = new System.Windows.Forms.TextBox();
      this.pinButton = new System.Windows.Forms.Button();
      this.ScreenshotFolderSelectDialog = new System.Windows.Forms.FolderBrowserDialog();
      this.folderSelectButton = new System.Windows.Forms.Button();
      this.ScreenshotFolderSelectBox = new System.Windows.Forms.TextBox();
      this.SuspendLayout();
      // 
      // startAuthButton
      // 
      this.startAuthButton.Location = new System.Drawing.Point(12, 12);
      this.startAuthButton.Name = "startAuthButton";
      this.startAuthButton.Size = new System.Drawing.Size(280, 83);
      this.startAuthButton.TabIndex = 0;
      this.startAuthButton.Text = "認証開始";
      this.startAuthButton.UseVisualStyleBackColor = true;
      this.startAuthButton.Click += new System.EventHandler(this.startAuthButton_Click);
      // 
      // pinInputBox
      // 
      this.pinInputBox.AllowDrop = true;
      this.pinInputBox.ImeMode = System.Windows.Forms.ImeMode.Alpha;
      this.pinInputBox.Location = new System.Drawing.Point(12, 101);
      this.pinInputBox.Name = "pinInputBox";
      this.pinInputBox.Size = new System.Drawing.Size(174, 19);
      this.pinInputBox.TabIndex = 1;
      // 
      // pinButton
      // 
      this.pinButton.Location = new System.Drawing.Point(192, 101);
      this.pinButton.Name = "pinButton";
      this.pinButton.Size = new System.Drawing.Size(100, 19);
      this.pinButton.TabIndex = 2;
      this.pinButton.Text = "PIN決定";
      this.pinButton.UseVisualStyleBackColor = true;
      this.pinButton.Click += new System.EventHandler(this.pinButton_Click);
      // 
      // folderSelectButton
      // 
      this.folderSelectButton.Location = new System.Drawing.Point(192, 126);
      this.folderSelectButton.Name = "folderSelectButton";
      this.folderSelectButton.Size = new System.Drawing.Size(100, 19);
      this.folderSelectButton.TabIndex = 3;
      this.folderSelectButton.Text = "画像保存フォルダ選択";
      this.folderSelectButton.UseVisualStyleBackColor = true;
      this.folderSelectButton.Click += new System.EventHandler(this.folderSelectButton_Click);
      // 
      // ScreenshotFolderSelectBox
      // 
      this.ScreenshotFolderSelectBox.Enabled = false;
      this.ScreenshotFolderSelectBox.Location = new System.Drawing.Point(13, 127);
      this.ScreenshotFolderSelectBox.Name = "ScreenshotFolderSelectBox";
      this.ScreenshotFolderSelectBox.Size = new System.Drawing.Size(173, 19);
      this.ScreenshotFolderSelectBox.TabIndex = 4;
      // 
      // Setting
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(304, 154);
      this.Controls.Add(this.ScreenshotFolderSelectBox);
      this.Controls.Add(this.folderSelectButton);
      this.Controls.Add(this.pinButton);
      this.Controls.Add(this.pinInputBox);
      this.Controls.Add(this.startAuthButton);
      this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
      this.Name = "Setting";
      this.Text = "Setting("+ System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString()+")";
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Button startAuthButton;
    private System.Windows.Forms.TextBox pinInputBox;
    private System.Windows.Forms.Button pinButton;
    private System.Windows.Forms.FolderBrowserDialog ScreenshotFolderSelectDialog;
    private System.Windows.Forms.Button folderSelectButton;
    private System.Windows.Forms.TextBox ScreenshotFolderSelectBox;
  }
}