   M S B A M L   `   `   `  ����N  JMonopoly Server, Version=0.7.0.20220, Culture=neutral, PublicKeyToken=null1clr-namespace:Monopoly_ServerMonopoly_Server  5   	        Monopoly_Server.MainWindow   5   	   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   /localclr-namespace:Monopoly_Server     5   	   -   5          � Title$0  *MonopoliumServer (V.20022022 For 0.7-Pre3)��5   	   $	��300��6<   $	��674��6I   .��5	      � $��0,0,2,2q�6   .+�5
   
   �� -   5         ��Name$ Button_StartServer��5
      $��Uruchom server��6-   $��	10,10,0,0q�6F   $	��120��6Y   $
��Left=�6�   $	��Top=�6�   $��24��6�   �� 5   
   -   5         ��Name$ Label_ServerStatus��5      $�� ��6,   $��Center=�67   $��144,10,10,0q�6T   $	��Top=�6i   $	��236��6�   $��24��6�   $��10(�6�   �� 5   
   -   5         ��Name$ ListBox_Players��5      $
��Left=�6+   $	��218��6F   $��10,39,260,0q�6S   $	��Top=�6h   $	��120��6�   �� 5   
   -   5        $ ListBox_PlayboardTheme��5      $
��Left=�62   $	��193��6M   $��144,39,10,0q�6Z   $	��Top=�6o   $	��236��6�   $��Visible=�6�   {�.  .@�5      �� $��Monopoly Standard��6   5      �� 5   
   -   5        $ Button_StartGame��5      $��Start��6+   $��Right=�6;   $��
0,237,10,0q�6W   $	��Top=�6k   $��75��6�   {�.  6�   �� 5   
   $
��Left=�6   $	��193��62   $��
385,39,0,0q�6?   $	��Top=�6S   $	��269��6k   ��. .��5      �� -   5         ��Name$ 	ServerLog��5      $
��Left=�6+   $
��Wrap=�6F   $	��Top=�6Z   $	��269��6r   ��. 6~   5      5      5      