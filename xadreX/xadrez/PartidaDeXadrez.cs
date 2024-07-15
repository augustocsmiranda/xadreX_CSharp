using System;
using tabuleiro;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace xadrez
{
    class PartidaDeXadrez
    {
        public Tabuleiro tab { get; private set; }
        public int turno { get; private set; }
        public Cor jogadorAtual { get; private set; }
        public bool terminada { get; private set; }
        private HashSet<Peca> pecas;
        private HashSet<Peca> capturadas;
        public bool xeque { get; private set; }

        public PartidaDeXadrez() {
            tab = new Tabuleiro(8, 8);
            turno = 1;
            jogadorAtual = Cor.Branca;
            terminada = false;
            xeque = false;
            pecas = new HashSet<Peca>();
            capturadas = new HashSet<Peca>();
            colocarPecas();

        }

        public Peca executaMovimento(Posicao origem, Posicao destino)
        {
            Peca p = tab.retirarPeca(origem);
            p.incrementarQteMovimentos();
            Peca pecaCaputurada = tab.retirarPeca(destino);
            tab.colocarPeca(p, destino);
            if(pecaCaputurada != null) { 
                capturadas.Add(pecaCaputurada);
            }
            return pecaCaputurada;

        }

        public void desfazMovimento(Posicao origem, Posicao destino, Peca pecaCapturada)
        {
            Peca p = tab.retirarPeca(destino);
            p.decrementarQteMovimentos();
            if (pecaCapturada != null)
            {
                tab.colocarPeca(pecaCapturada, destino);
                capturadas.Remove(pecaCapturada);
            }
            tab.colocarPeca(p, origem);
        }
        public void realizaJogada(Posicao origem, Posicao destino)
        {
            Peca pecaCapturada = executaMovimento(origem, destino);

            if (estaEmXeque(jogadorAtual))
            {
                desfazMovimento(origem, destino, pecaCapturada);
                throw new TabuleiroException("Você não pode se colocar em Xeque");
            }

            if (estaEmXeque(adversaria(jogadorAtual)))
            {
                xeque = true;
            }
            else
            {
                xeque = false;
            }

            if (testeXequeMate(adversaria(jogadorAtual)))
            {
                terminada = true;
            }
            else
            {
                turno++;
                mudaJogador();
            }
        }

        public void validarPosicaoDeOrigem(Posicao pos)
        {
            if(tab.peca(pos) == null)
            {
                throw new TabuleiroException("Não Existe peça na posição de Origem escolhida!");
            }
            if(jogadorAtual != tab.peca(pos).cor)
            {
                throw new TabuleiroException("A peça de Origem escolhida não é sua!");
            }
            if (!tab.peca(pos).existeMovimentosPossiveis())
            {
                throw new TabuleiroException("Não há movimentos possiveis para a peça de Origem escolhida!");
            }
        }

        public void validarPosicaoDeDestino(Posicao origem, Posicao destino)
        {
            if (!tab.peca(origem).podeMoverPara(destino))
            {
                throw new TabuleiroException("Posição de Destino inválida!");
            }
        }

        private void mudaJogador()
        {
            if (jogadorAtual == Cor.Branca)
            {
                jogadorAtual = Cor.Preta;
            }
            else
            {
                jogadorAtual = Cor.Branca;
            }
        }

        public HashSet<Peca> pecasCapturadas(Cor cor)
        {
            HashSet<Peca> aux = new HashSet<Peca>();
            foreach (Peca x in capturadas)
            {
                if(x.cor == cor)
                {
                    aux.Add(x);
                }
            }
            return aux;
        }

        public HashSet<Peca> pecasEmJogos(Cor cor)
        {
            HashSet<Peca> aux = new HashSet<Peca>();
            foreach (Peca x in pecas)
            {
                if (x.cor == cor)
                {
                    aux.Add(x);
                }
            }
            aux.ExceptWith(pecasCapturadas(cor));
            return aux;
        }

        private Peca rei(Cor cor)
        {
            foreach(Peca x in pecasEmJogos(cor))
            {
                if(x is Rei)
                {
                    return x;
                }
            }
            return null;
        }
        
        public bool estaEmXeque(Cor cor)
        {
            Peca R = rei(cor);
            if(R == null)
            {
                throw new Exception("Não tem Rei da Cor" + cor + "no tabuleiro");
            }
            foreach(Peca x in pecasEmJogos(adversaria(cor)))
            {
                bool[,] mat = x.movimentosPossiveis();
                if (mat[R.posicao.linha, R.posicao.coluna])
                {
                    return true;
                }
            }
            return false;
        }

        public bool testeXequeMate(Cor cor)
        {
            if (!estaEmXeque(cor))
            {
                return false;
            }
            foreach (Peca x in pecasEmJogos(cor))
            {
                bool[,] mat = x.movimentosPossiveis();
                for (int i = 0; i < tab.linhas; i++)
                {
                    for (int j = 0; j < tab.colunas; j++)
                    {
                        if(mat[i, j])
                        {
                            Posicao origem = x.posicao;
                            Posicao destino = new Posicao(i, j);
                            Peca pecaCapturada = executaMovimento(origem,destino);
                            bool testeXeque = estaEmXeque(cor);
                            desfazMovimento(origem, destino, pecaCapturada);
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
        private Cor adversaria(Cor cor)
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
        public void colocarNovaPeca(char coluna, int linha, Peca peca)
        {
            tab.colocarPeca(peca, new PosicaoXadrez(coluna, linha).toPosicao());
            pecas.Add(peca);
        }

        private void colocarPecas() {
            colocarNovaPeca('c', 1, new Torre(tab, Cor.Branca));
            colocarNovaPeca('d', 1, new Rei(tab, Cor.Branca));
            colocarNovaPeca('h', 7, new Torre(tab, Cor.Branca));

            colocarNovaPeca('a', 8, new Rei(tab, Cor.Preta));
            colocarNovaPeca('b', 8, new Torre(tab, Cor.Preta));
            //colocarNovaPeca('c', 1, new Torre(tab, Cor.Branca));
            //colocarNovaPeca('c', 2, new Torre(tab, Cor.Branca));
            //colocarNovaPeca('d', 2, new Torre(tab, Cor.Branca));
            //colocarNovaPeca('e', 2, new Torre(tab, Cor.Branca));
            //colocarNovaPeca('e', 1, new Torre(tab, Cor.Branca));
            //colocarNovaPeca('d', 1, new Rei(tab, Cor.Branca));

            //colocarNovaPeca('c', 7, new Torre(tab, Cor.Preta));
            //colocarNovaPeca('c', 8, new Torre(tab, Cor.Preta));
            //colocarNovaPeca('d', 7, new Torre(tab, Cor.Preta));
            //colocarNovaPeca('e', 7, new Torre(tab, Cor.Preta));
            //colocarNovaPeca('e', 8, new Torre(tab, Cor.Preta));
            //colocarNovaPeca('d', 8, new Rei(tab, Cor.Preta));
        }

    }
}
