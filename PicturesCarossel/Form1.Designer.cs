

namespace PicturesCarossel;

partial class Form1
{
    /// <summary>
    ///  Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    ///  Clean up any resources being used.
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
    ///  Required method for Designer support - do not modify
    ///  the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        comboBox1 = new ComboBox();
        button1 = new Button();
        comboBox2 = new ComboBox();
        button2 = new Button();
        panel1 = new Panel();
        SuspendLayout();
        // 
        // comboBox1
        // 
        comboBox1.FormattingEnabled = true;
        comboBox1.Location = new Point(12, 12);
        comboBox1.Name = "comboBox1";
        comboBox1.Size = new Size(205, 23);
        comboBox1.TabIndex = 2;
        comboBox1.SelectedIndexChanged += comboBox1_SelectedIndexChanged;
        // 
        // button1
        // 
        button1.Location = new Point(434, 12);
        button1.Name = "button1";
        button1.Size = new Size(75, 23);
        button1.TabIndex = 3;
        button1.Text = "button1";
        button1.UseVisualStyleBackColor = true;
        button1.Click += button1_Click;
        // 
        // comboBox2
        // 
        comboBox2.FormattingEnabled = true;
        comboBox2.Location = new Point(223, 12);
        comboBox2.Name = "comboBox2";
        comboBox2.Size = new Size(205, 23);
        comboBox2.TabIndex = 4;
        comboBox2.SelectedIndexChanged += comboBox2_SelectedIndexChanged;
        // 
        // button2
        // 
        button2.Location = new Point(515, 12);
        button2.Name = "button2";
        button2.Size = new Size(75, 23);
        button2.TabIndex = 5;
        button2.Text = "button2";
        button2.UseVisualStyleBackColor = true;
        button2.Click += LoadFolder_Btn_Click;
        // 
        // panel1
        // 
        panel1.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        panel1.AutoScroll = true;
        panel1.Location = new Point(12, 41);
        panel1.Name = "panel1";
        panel1.Size = new Size(1347, 150);
        panel1.TabIndex = 6;
        // 
        // Form1
        // 
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        AutoScroll = true;
        ClientSize = new Size(1371, 203);
        Controls.Add(panel1);
        Controls.Add(button2);
        Controls.Add(comboBox2);
        Controls.Add(button1);
        Controls.Add(comboBox1);
        Name = "Form1";
        Text = "Form1";
        ResumeLayout(false);
    }

    #endregion
    private ComboBox comboBox1;
    private Button button1;
    private ComboBox comboBox2;
    private Button button2;
    private Panel panel1;
}
