
using tabuleiro;
using Xadrez;
using xadrez_console;



Tabuleiro tab = new Tabuleiro(8, 8);
try
{
    tab.ColocarPeca(new Torre(tab, Cor.Preta), new Posicao(0, 0));
    tab.ColocarPeca(new Torre(tab, Cor.Preta), new Posicao(1, 3));
    tab.ColocarPeca(new Rei(tab, Cor.Preta), new Posicao(0, 9));

Tela.ImprimirTabuleiro(tab);
}
catch(TabuleiroException e)
{
    Console.WriteLine(e.Message);
}

Console.ReadLine();