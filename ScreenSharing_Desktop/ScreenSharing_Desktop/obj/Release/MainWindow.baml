   M S B A M L   `   `   `  ���� N  JScreen Sharing App, Version=2.0.27.0, Culture=neutral, PublicKeyToken=null=#clr-namespace:ScreenSharing_DesktopScreenSharing_Desktop  &     ScreenSharing_Desktop.MainWindow   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     @d2http://schemas.microsoft.com/expression/blend/2008     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5local#clr-namespace:ScreenSharing_Desktop     -     � Title$  Screen Sharing App��$
��1080��$
��1920�� � 
ResizeMode$ CanResizeWithGrip=� � WindowState$ 	Maximized=�.���  � RowDefinitions �� $��35 ��� $	��20* �.+��� $��0��$��2��
���   �.+��� -    ��Name$ imageBox��|�. $��Uniform=�$
��1025���� -   $ Img_OpenBar��$��0��$��35��$��35��$��Icons/smallTV.ico��� -    �Name$ stc_ControlBar��$��0��$��0,0,0,0q� J� Opacity$	 0.8Q�
���OO/� � ColumnDefinitions �� $��2* ��� $��4* �.+���  �� Orientation$ 
Horizontal=�$��0��.+��� -   	 ��Name$	 	btn_Share��$��Share��$��5,0,0,0q�$
��Left=�$��Center=�
�����F�$��20(�$��DemiBold"�
�������|�.  �� -   $	 btn_Connect��$��Connect��$��5,0,0,0q�$��Center=�
�������$��20(�$��DemiBold"�
�������|�.  �� -   $	 Btn_Refresh��$��35��$��35��
�������$��	10,0,10,0q�.���� $��Icons/Refresh.png��$
��Fill=��� -   
 ��Name$
 txt_IP�� �� 
IsEditable . $��Center=�$��Center=�
���   �
������ $��1q�$��DemiBold"�$��20(�$��70�� �� Text$ 192.168.1.37��� $��1�� �� $��* ��� $��* ��� $��* �.+�� $��0��.+��� $ 
Horizontal=�$��10,0,0,0q�$��Center=�$��Center=�.+��� $��Speed:��
�������$��20(�$��DemiBold"��� -	    ��Name$ 	lbl_Speed��$��	2.56 MB/s��$��20(�$��DemiBold"�
�������� $��1��.+��� $ 
Horizontal=�$��0,0,0,0q�$
��Left=�$��Center=�.+��� $
��FPS:��
�������$��20(�$��DemiBold"��� -
   $ lbl_FPS��$��30��$��20(�$��DemiBold"�
��������� $��Ping:��
�������$��18(�$��DemiBold"�$��5,0,0,0q��� -   $ lbl_Ping��$��30��$��20(�$��DemiBold"�
��������� -   $	 Btn_RotateImage��$��Rotate��$��15,0,0,0q�$��20(�� $��2��$��Right=�.+��� $ 
Horizontal=�$��0,0,10,0q�$��Center=�.+��� -   $ Led_CommandsReceived��$��20��$��20��
���  ���� -    ��Name$ chc_AutoShare��$��
Auto Share��$��15,0,0,0q�$��Center=�
�������$��15(�$��DemiBold"�|�.  �� -   $ chc_EnableControls��$��Control Enable��$��15,0,0,0q�$��Center=�
�������$��15(�$��DemiBold"�|�.  