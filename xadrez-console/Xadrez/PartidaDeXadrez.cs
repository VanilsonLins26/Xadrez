﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Tasks;
using tabuleiro;
using xadrez_console.Xadrez;

namespace Xadrez
{
    internal class PartidaDeXadrez
    {
        public Tabuleiro tab { get; private set; }
        public int Turno { get; private set; }
        public Cor JogadorAtual { get; private set; }
        public bool Terminada { get; private set; }
        public HashSet<Peca> Pecas;
        public HashSet<Peca> Capturadas;
        public bool xeque { get; private set; }
        public Peca VulneravelEnPassant { get; private set; }

        public PartidaDeXadrez()
        {
            tab = new Tabuleiro(8, 8);
            Turno = 1;
            JogadorAtual = Cor.Branca;
            Terminada = false;
            xeque = false;
            VulneravelEnPassant = null;
            Pecas = new HashSet<Peca>();
            Capturadas = new HashSet<Peca>();
            ColocarPecas();

        }

        public Peca ExecutaMovimento(Posicao origem, Posicao destino)
        {
            Peca p = tab.RetirarPeca(origem);
            p.IncrementarQteMovimentos();
           Peca PecaCapturada = tab.RetirarPeca(destino);
            tab.ColocarPeca(p, destino);
            if (PecaCapturada != null)
            {
                Capturadas.Add(PecaCapturada);
            }

            // #jogadaespecial roque pequeno
            if(p is Rei && destino.Coluna == origem.Coluna + 2)
            {
                Posicao OrigemT = new Posicao(origem.Linha, origem.Coluna + 3);
                Posicao DestinoT = new Posicao(origem.Linha, origem.Coluna + 1);
                Peca T = tab.RetirarPeca(OrigemT);
                T.IncrementarQteMovimentos();
                tab.ColocarPeca(T, DestinoT);
            }

            // #jogadaespecial roque grande
            if (p is Rei && destino.Coluna == origem.Coluna - 2)
            {
                Posicao OrigemT = new Posicao(origem.Linha, origem.Coluna - 4);
                Posicao DestinoT = new Posicao(origem.Linha, origem.Coluna - 1);
                Peca T = tab.RetirarPeca(OrigemT);
                T.IncrementarQteMovimentos();
                tab.ColocarPeca(T, DestinoT);
            }

            // #jogadaespecial en passant
            if(p is Peao)
            {
                if(origem.Coluna != destino.Coluna && PecaCapturada == null)
                {
                    Posicao PosP;
                    if(p.Cor == Cor.Branca)
                    {
                        PosP = new Posicao(destino.Linha + 1, destino.Coluna);
                    }
                    else
                    {
                        PosP = new Posicao(destino.Linha - 1, destino.Coluna);
                    }
                    PecaCapturada = tab.RetirarPeca(PosP);
                    Capturadas.Add(PecaCapturada);
                }
            }

            return PecaCapturada;
        }

        public void DesfazMovimento(Posicao origem, Posicao destino, Peca pecaCapturada)
        {
            Peca p = tab.RetirarPeca(destino);
            p.DecrementarQteMovimentos();
            if(pecaCapturada != null)
            {
                tab.ColocarPeca(pecaCapturada, destino);
                Capturadas.Remove(pecaCapturada);
            }
            tab.ColocarPeca(p, origem);

            // #jogadaespecial roque pequeno
            if (p is Rei && destino.Coluna == origem.Coluna + 2)
            {
                Posicao OrigemT = new Posicao(origem.Linha, origem.Coluna + 3);
                Posicao DestinoT = new Posicao(origem.Linha, origem.Coluna + 1);
                Peca T = tab.RetirarPeca(DestinoT);
                T.DecrementarQteMovimentos();
                tab.ColocarPeca(T, OrigemT);
            }

            // #jogadaespecial roque grande
            if (p is Rei && destino.Coluna == origem.Coluna - 2)
            {
                Posicao OrigemT = new Posicao(origem.Linha, origem.Coluna - 4);
                Posicao DestinoT = new Posicao(origem.Linha, origem.Coluna - 1);
                Peca T = tab.RetirarPeca(DestinoT);
                T.DecrementarQteMovimentos();
                tab.ColocarPeca(T, OrigemT);
            }

            // #jogadaespecial en passant
            if(p is Peao)
            {
                if(origem.Coluna != destino.Coluna && pecaCapturada == VulneravelEnPassant)
                {
                    Peca peao = tab.RetirarPeca(destino);
                    Posicao Posp;
                    if(p.Cor == Cor.Branca)
                    {
                        Posp = new Posicao(3, destino.Coluna);
                    }

                    else
                    {
                        Posp = new Posicao(4, destino.Coluna);
                    }
                    tab.ColocarPeca(peao, Posp);
                }
            }

        }

        public void RealizaJogada(Posicao origem,Posicao destino)
        {
           Peca Pecacapturada = ExecutaMovimento(origem, destino);

            if (EstaEmXeque(JogadorAtual))
            {
                DesfazMovimento(origem, destino, Pecacapturada);
                throw new TabuleiroException("Você não pode se colocar em xeque!");
            }

            Peca p = tab.Peca(destino);

            // #jogadaespecial promocao
            if(p is Peao)
            {
                if((p.Cor == Cor.Branca && destino.Linha == 0) || (p.Cor == Cor.Preta && destino.Linha == 7))
                {
                    p = tab.RetirarPeca(destino);
                    Pecas.Remove(p);
                    Peca dama = new Dama(tab, p.Cor);
                    tab.ColocarPeca(dama, destino);
                    Pecas.Add(dama);
                }
            }


            if (EstaEmXeque( (JogadorAtual)))
            {
                xeque = true;
            }
            else
            {
                xeque = false;
            }

            if (TesteXequemate(Adversaria(JogadorAtual)))
            {
                Terminada = true;
            }

            else
            {
                Turno++;
                MudaJogador();
            }

            
            if(p is Peao && (destino.Linha == origem.Linha -2 || destino.Linha == origem.Linha + 2))
            {
                VulneravelEnPassant = p;
            }
            else
            {
                VulneravelEnPassant = null;
            }
        }

       

        public void ValidarPosicaoDeOrigem(Posicao pos)
        {
            if(tab.Peca(pos) == null)
            {
                throw new TabuleiroException("Não existe peça na posição de origem escolhida");
            }
            if(JogadorAtual != tab.Peca(pos).Cor)
            {
                throw new TabuleiroException("A peça de origem esolhida não é sua!");
            }
            if (!tab.Peca(pos).ExisteMovimentosPossiveis())
            {
                throw new TabuleiroException("Não há movimentos possíveis para a peça de origem escolhida!");
            }
        }

        public void ValidarPosicaoDeDestino(Posicao origem, Posicao destino)
        {
            if (!tab.Peca(origem).MovimentoPossivel(destino))
            {
                throw new TabuleiroException("Posição de destino inválida!");
            }
        }
        private void MudaJogador()
        {
            if(JogadorAtual == Cor.Branca)
            {
                JogadorAtual = Cor.Preta;

            }
            else
            {
                JogadorAtual = Cor.Branca;
            }
        }
        public  HashSet<Peca> PecasCapturadas(Cor cor)
        {
            HashSet<Peca> aux = new HashSet<Peca>();
            foreach(Peca x in Capturadas)
            {
                if(x.Cor == cor)
                {
                    aux.Add(x);
                }
            }
            return aux;

        }

        public HashSet<Peca> PecasEmJogo(Cor cor)
        {
            HashSet<Peca> aux = new HashSet<Peca>();
            foreach (Peca x in Pecas)
            {
                if (x.Cor == cor)
                {
                    aux.Add(x);
                }

            }
            aux.ExceptWith(PecasCapturadas(cor));
            return aux;
        }
        
        public bool TesteXequemate(Cor cor)
        {
            if (!EstaEmXeque(cor))
            {
                return false;
            }
            foreach(Peca x in PecasEmJogo(cor))
            {
                bool[,] mat = x.MovimentosPossiveis();
                for(int i = 0; i < tab.Linhas; i++)
                {
                    for(int j = 0; j < tab.Colunas; j++)
                    {
                        if (mat[i, j])
                        {
                            Posicao origem = x.Posicao;
                            Posicao destino = new Posicao(i, j);
                            Peca pecaCapturada = ExecutaMovimento(origem, destino);
                            bool testeXeque = EstaEmXeque(cor);
                            DesfazMovimento(origem, destino, pecaCapturada);
                            if (!testeXeque)
                            {
                                return false;
                            }
                        }
                    }
                }
            }
            return true;
        }

        private Cor Adversaria(Cor cor)
        {
            if(cor == Cor.Branca)
            {
                return Cor.Preta;
            }
            else
            {
                return Cor.Branca;
            }
        }
        
        private Peca Rei(Cor cor)
        {
            foreach(Peca x in PecasEmJogo(cor))
            {
                if(x is Rei)
                {
                    return x;
                }
            }
            return null;
        }

        public bool EstaEmXeque(Cor cor)
        {
            Peca R = Rei(cor);
            if(R == null)
            {
                throw new TabuleiroException("Não tem rei da cor " + cor + "no tabuleiro!");
            }
            foreach(Peca x in PecasEmJogo(Adversaria(cor)))
            {
                bool[,] mat = x.MovimentosPossiveis();
                if (mat[R.Posicao.Linha, R.Posicao.Coluna])
                {
                    return true;
                }
            }
            return false;
        }

        public void ColocarNovaPeca(char coluna, int linha, Peca peca)
        {
            tab.ColocarPeca(peca, new PosicaoXadrez(coluna, linha).ToPosicao());
            Pecas.Add(peca);

        }



        private void ColocarPecas()
        {
            ColocarNovaPeca('a', 1,new Torre(tab, Cor.Branca));
            ColocarNovaPeca('b', 1, new Cavalo(tab, Cor.Branca));
            ColocarNovaPeca('c', 1, new Bispo(tab, Cor.Branca));
            ColocarNovaPeca('d', 1, new Dama(tab, Cor.Branca));
            ColocarNovaPeca('e', 1, new Rei(tab, Cor.Branca, this));
            ColocarNovaPeca('f', 1, new Bispo(tab, Cor.Branca));
            ColocarNovaPeca('g', 1, new Cavalo(tab, Cor.Branca));
            ColocarNovaPeca('h', 1, new Torre(tab, Cor.Branca));
            ColocarNovaPeca('a', 2, new Peao(tab, Cor.Branca, this));
            ColocarNovaPeca('b', 2, new Peao(tab, Cor.Branca, this));
            ColocarNovaPeca('c', 2, new Peao(tab, Cor.Branca, this));
            ColocarNovaPeca('d', 2, new Peao(tab, Cor.Branca, this));
            ColocarNovaPeca('e', 2, new Peao(tab, Cor.Branca, this));
            ColocarNovaPeca('f', 2, new Peao(tab, Cor.Branca, this));
            ColocarNovaPeca('g', 2, new Peao(tab, Cor.Branca, this));
            ColocarNovaPeca('h', 2, new Peao(tab, Cor.Branca, this));



            ColocarNovaPeca('a', 8, new Torre(tab, Cor.Preta));
            ColocarNovaPeca('b', 8, new Cavalo(tab, Cor.Preta));
            ColocarNovaPeca('c', 8, new Bispo(tab, Cor.Preta));
            ColocarNovaPeca('d', 8, new Dama(tab, Cor.Preta));
            ColocarNovaPeca('e', 8, new Rei(tab, Cor.Preta, this));
            ColocarNovaPeca('f', 8, new Bispo(tab, Cor.Preta));
            ColocarNovaPeca('g', 8, new Cavalo(tab, Cor.Preta));
            ColocarNovaPeca('h', 8, new Torre(tab, Cor.Preta));
            ColocarNovaPeca('a', 7, new Peao(tab, Cor.Preta, this));
            ColocarNovaPeca('b', 7, new Peao(tab, Cor.Preta, this));
            ColocarNovaPeca('c', 7, new Peao(tab, Cor.Preta, this));
            ColocarNovaPeca('d', 7, new Peao(tab, Cor.Preta, this));
            ColocarNovaPeca('e', 7, new Peao(tab, Cor.Preta, this));
            ColocarNovaPeca('f', 7, new Peao(tab, Cor.Preta, this));
            ColocarNovaPeca('g', 7, new Peao(tab, Cor.Preta, this));
            ColocarNovaPeca('h', 7, new Peao(tab, Cor.Preta, this));



        }
    }
}
