using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Prefrlanje
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        AvtorskaEntities _avtorskaEntities=new AvtorskaEntities();
        MladinskaEntities _mladinskaEntities=new MladinskaEntities();

        private void buttonAvtorskiClenCLENOVI_A_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            int i = 0;
            foreach (var ca in _mladinskaEntities.CLENOVI_A)
            {
                CLENOVI_A ca1 = ca;
                if (_avtorskaEntities.AvtorskiClenovi.Count(c => c.ClenskiBroj == ca1.BROJ) != 0) continue;
                var ac = new AvtorskiClenovi
                             {
                                 Adresa = ca.ADRESA,
                                 ClenskiBroj = ca.BROJ,
                                 MaticenBroj = ca.MB,
                                 LicnaKarta = ca.LICNAKARTA,
                                 Telefon = ca.TELEFONI
                             };

                if (!String.IsNullOrWhiteSpace(ca.POL))
                {
                    if (ca.POL.Contains("m"))
                        ac.Pol = _avtorskaEntities.Pol.FirstOrDefault(p => p.NazivPol.Contains("м"));
                    else if (ca.POL.Contains("z"))
                        ac.Pol = _avtorskaEntities.Pol.FirstOrDefault(p => p.NazivPol.Contains("ж"));
                }

                DateTime dateTime;
                if (DateTime.TryParse(ca.DATUM_CLEN, out dateTime))
                    if (dateTime >= Convert.ToDateTime("1753/1/1") && dateTime <= DateTime.MaxValue)
                        ac.DatumNaZaclenuvanje = dateTime;
                
                _avtorskaEntities.AddToAvtorskiClenovi(ac);
                i++;
            }
            _avtorskaEntities.SaveChanges();
            Cursor.Current = Cursors.Default;
            MessageBox.Show("CLENOVI_A => AvtorskiClenovi : "+i);
        }

        private void buttonBankiAC_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            int i = 0;
            foreach (var ca in _mladinskaEntities.CLENOVI_A)
            {
                if (String.IsNullOrWhiteSpace(ca.BANKA)) continue;
                var ca1 = ca;
                if (_avtorskaEntities.Banka.Count(b => b.NazivBanka.Contains(ca1.BANKA.Trim())) != 0) continue;
                var banka = new Banka {NazivBanka = ca.BANKA};
                _avtorskaEntities.AddToBanka(banka);
                _avtorskaEntities.SaveChanges();
                i++;
            }
            Cursor.Current = Cursors.Default;
            MessageBox.Show("CLENOVI_A.BANKA => Banka : " + i);
        }

        private void button1TEKOVNA_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            int i = 0;
            var list = new List<ZiroSmetkaAvtor>();
            foreach (var ca in _mladinskaEntities.CLENOVI_A)
            {
                if (String.IsNullOrWhiteSpace(ca.TEKOVNA)) continue;
                CLENOVI_A ca2 = ca;
                if (_avtorskaEntities.ZiroSmetkaAvtor.Count(z => z.AvtorskiClenovi.ClenskiBroj == ca2.BROJ) != 0)
                    continue;
                var ziroSmetkaAvtor = new ZiroSmetkaAvtor
                                          {
                                              ZiroSmetka = ca.TEKOVNA,
                                              AvtorskiClenovi =
                                                  _avtorskaEntities.AvtorskiClenovi.FirstOrDefault(
                                                      c => c.ClenskiBroj == ca2.BROJ)
                                          };
                if (!String.IsNullOrWhiteSpace(ca2.BANKA))
                {
                    if (_avtorskaEntities.Banka.Count(b => b.NazivBanka.Contains(ca2.BANKA.Trim())) == 0)
                    {
                        var banka = new Banka {NazivBanka = ca2.BANKA};
                        _avtorskaEntities.AddToBanka(banka);
                        _avtorskaEntities.SaveChanges();
                        ziroSmetkaAvtor.Banka = banka;
                    }
                    else
                    {
                        var banka = _avtorskaEntities.Banka
                            .FirstOrDefault(b => b.NazivBanka.Contains(ca2.BANKA.Trim()));
                        ziroSmetkaAvtor.Banka = banka;
                    }
                }
                else
                {
                    var banka = _avtorskaEntities.Banka.FirstOrDefault(b => b.NazivBanka.Trim().Equals(""));
                    ziroSmetkaAvtor.Banka = banka;
                }
                _avtorskaEntities.AddToZiroSmetkaAvtor(ziroSmetkaAvtor);
                list.Add(ziroSmetkaAvtor);
                i++;
                _avtorskaEntities.SaveChanges();
            }
            Cursor.Current = Cursors.Default;
            MessageBox.Show("CLENOVI_A.TEKOVNA => ziroSmetkaAvtor : " + i);
        }

        private void button1ime_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            int c = 0;
            foreach (var ca in _mladinskaEntities.CLENOVI_A)
            {
                var ca1 = ca;
                foreach (var ac in _avtorskaEntities.AvtorskiClenovi.Where(ac => ac.ClenskiBroj == ca1.BROJ))
                {
                    if (String.IsNullOrWhiteSpace(ac.Ime) && String.IsNullOrWhiteSpace(ac.Prezime))
                    {
                        string[] strings = ca.PREZIMEIME.Trim().Split(' ');
                        int b = strings.Count();
                        var prezime = new StringBuilder("");
                        var ime = new StringBuilder("");
                        if (b > 2)
                        {
                            for (int i = 0; i < b / 2 + 1; i++)
                                prezime.Append(strings[i]+" ");
                            for (int i = b / 2 + 1; i < b; i++)
                                ime.Append(strings[i]+" ");
                        }
                        else
                        {
                            prezime = new StringBuilder(strings[0]);
                            ime = b > 1 ? new StringBuilder(strings[1]) : new StringBuilder("");
                        }
                        ac.Prezime = prezime.ToString();
                        ac.Ime = ime.ToString();
                        c++;
                    }
                }
            }
            _avtorskaEntities.SaveChanges();
            Cursor.Current = Cursors.Default;
            MessageBox.Show("CLENOVI_A.PREZIMEIME => AvtorskiClenovi.Ime .Prezime : " + c);
        }

        private void buttonIzveduvaciCLENOVI_D_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            int i = 0;
            foreach (var cd in _mladinskaEntities.CLENOVI_D)
            {
                var cd1 = cd;
                if (_avtorskaEntities.Izveduvaci.Count(c => c.Broj == cd1.BROJ) != 0) continue;
                var izv = new Izveduvaci
                {
                    Adresa = cd.ADRESA,
                    Broj = cd.BROJ,
                    MaticenBroj = cd.MB,
                    LicnaKarta = cd.LICNAKARTA,
                    Telefon = cd.TELEFONI
                };

                if (!String.IsNullOrWhiteSpace(cd.POL))
                {
                    if (cd.POL.Contains("m"))
                        izv.Pol = _avtorskaEntities.Pol.FirstOrDefault(p => p.NazivPol.Contains("м"));
                    else if (cd.POL.Contains("z"))
                        izv.Pol = _avtorskaEntities.Pol.FirstOrDefault(p => p.NazivPol.Contains("ж"));
                }

                DateTime dateTime;
                if (DateTime.TryParse(cd.DATUM_CLEN, out dateTime))
                    if (dateTime >= Convert.ToDateTime("1753/1/1") && dateTime <= DateTime.MaxValue)
                        izv.DatumNaZaclenuvanje = dateTime;

                _avtorskaEntities.AddToIzveduvaci(izv);
                i++;
            }
            _avtorskaEntities.SaveChanges();
            Cursor.Current = Cursors.Default;
            MessageBox.Show("CLENOVI_D => Izveduvaci : " + i);
        }

        private void buttonBankiIzveduvaciCLENOVI_D_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            int i = 0;
            foreach (var cd in _mladinskaEntities.CLENOVI_D)
            {
                if (String.IsNullOrWhiteSpace(cd.BANKA)) continue;
                var ca1 = cd;
                if (_avtorskaEntities.Banka.Count(b => b.NazivBanka.Contains(ca1.BANKA.Trim())) != 0) continue;
                var banka = new Banka { NazivBanka = cd.BANKA };
                _avtorskaEntities.AddToBanka(banka);
                _avtorskaEntities.SaveChanges();
                i++;
            }
            Cursor.Current = Cursors.Default;
            MessageBox.Show("CLENOVI_A.BANKA => Banka : " + i);
        }

        private void buttonimeIzveduvaciCLENOVI_D_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            int c = 0;
            foreach (var cd in _mladinskaEntities.CLENOVI_D)
            {
                var ca1 = cd;
                foreach (var izv in _avtorskaEntities.Izveduvaci.Where(ac => ac.Broj == ca1.BROJ))
                {
                    if (String.IsNullOrWhiteSpace(izv.Ime) && String.IsNullOrWhiteSpace(izv.Prezime))
                    {
                        string[] strings = cd.PREZIMEIME.Trim().Split(' ');
                        int b = strings.Count();
                        var prezime = new StringBuilder("");
                        var ime = new StringBuilder("");
                        if (b > 2)
                        {
                            for (int i = 0; i < b / 2 + 1; i++)
                                prezime.Append(strings[i] + " ");
                            for (int i = b / 2 + 1; i < b; i++)
                                ime.Append(strings[i] + " ");
                        }
                        else
                        {
                            prezime = new StringBuilder(strings[0]);
                            ime = b > 1 ? new StringBuilder(strings[1]) : new StringBuilder("");
                        }
                        izv.Prezime = prezime.ToString();
                        izv.Ime = ime.ToString();
                        c++;
                    }
                }
            }
            _avtorskaEntities.SaveChanges();
            Cursor.Current = Cursors.Default;
            MessageBox.Show("CLENOVI_A.PREZIMEIME => AvtorskiClenovi.Ime .Prezime : " + c);
        }

        private void buttonTEKOVNAIzveduvaciCLENOVI_D_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            int i = 0;
            var list = new List<ZiroSmetkaIzveduvac>();
            foreach (var ca in _mladinskaEntities.CLENOVI_D)
            {
                if (String.IsNullOrWhiteSpace(ca.TEKOVNA)) continue;
                var ca2 = ca;
                if (_avtorskaEntities.ZiroSmetkaIzveduvac.Count(z => z.Izveduvaci.Broj == ca2.BROJ) != 0)
                    continue;
                var ziroSmetkaizv = new ZiroSmetkaIzveduvac
                                        {
                                            ZiroSmetka = ca.TEKOVNA,
                                            Izveduvaci =
                                                _avtorskaEntities.Izveduvaci
                                                .FirstOrDefault(c => c.Broj == ca2.BROJ)
                                        };
                if (!String.IsNullOrWhiteSpace(ca2.BANKA))
                {
                    if (_avtorskaEntities.Banka.Count(b => b.NazivBanka.Contains(ca2.BANKA.Trim())) == 0)
                    {
                        var banka = new Banka { NazivBanka = ca2.BANKA };
                        _avtorskaEntities.AddToBanka(banka);
                        _avtorskaEntities.SaveChanges();
                        ziroSmetkaizv.Banka = banka;
                    }
                    else
                    {
                        var banka = _avtorskaEntities.Banka
                            .FirstOrDefault(b => b.NazivBanka.Contains(ca2.BANKA.Trim()));
                        ziroSmetkaizv.Banka = banka;
                    }
                }
                else
                {
                    var banka = _avtorskaEntities.Banka.FirstOrDefault(b => b.NazivBanka.Trim().Equals(""));
                    ziroSmetkaizv.Banka = banka;
                }
                _avtorskaEntities.AddToZiroSmetkaIzveduvac(ziroSmetkaizv);
                list.Add(ziroSmetkaizv);
                i++;
            }
            _avtorskaEntities.SaveChanges();
            Cursor.Current = Cursors.Default;
            MessageBox.Show("CLENOVI_A.TEKOVNA => ZiroSmetkaIzveduvac : " + i);
        }

        private void buttonDogovorNaDeloDOG_D_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            int i = 0;
            foreach (var dogd in _mladinskaEntities.DOG_D)
            {
                var dogd1 = dogd;
                if (_avtorskaEntities.DogovorNaDelo.Count(d => d.Broj == dogd1.BROJ && d.Godina == dogd1.GODINA) != 0)
                    continue;
                var delo = new DogovorNaDelo
                               {
                                   Broj = dogd1.BROJ,
                                   Kraj = dogd1.KRAJ,
                                   Dat_Do = dogd1.DAT_DO,
                                   Dat_Od = dogd1.DAT_OD,
                                   TipA = dogd1.TIPA,
                                   God_F = dogd1.GOD_F,
                                   Dog_F = dogd1.DOG_F,
                                   Godina = dogd1.GODINA,
                                   OpisClen2 = dogd1.OPISCLEN2,
                                   OpisRabota = dogd1.OPISRABOTA,
                                   prazno = dogd1.prazno,
                                   Bruto = (decimal?) dogd1.BRUTO,
                                   Neto = (decimal?) dogd1.NETO,
                                   Personalen = (decimal?) dogd1.PERSONALEN,
                                   Popust = (decimal?) dogd1.POPUST,
                                   Provizija = (decimal?) dogd1.PROVIZIJA,
                                   tempf = (decimal?) dogd1.tempf
                               };
                DateTime dateTime;
                if (DateTime.TryParse(dogd1.DATUM, out dateTime))
                    if (dateTime >= Convert.ToDateTime("1753/1/1") && dateTime <= DateTime.MaxValue)
                        delo.Datum = dateTime;

                var idf = dogd1.DOG_F ?? -1;
                string sdf = idf.ToString();
                if (idf != -1) delo.Firma = _avtorskaEntities.Firma.FirstOrDefault(f => f.BrFirma.Equals(sdf));

                _avtorskaEntities.AddToDogovorNaDelo(delo);
            _avtorskaEntities.SaveChanges();
            }
            Cursor.Current = Cursors.Default;
            MessageBox.Show("DOG_D => DogovorNaDelo : " + i);
        }

        private void buttonFakturiFAKTURI_D_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            int i = 0;
            foreach (var fd in _mladinskaEntities.FAKTURI_D)
            {
                var fd1 = fd;
                if (_avtorskaEntities.Fakturi.Count(d => d.Broj == fd1.BROJ && d.Godina == fd1.GODINA && fd1.prazno=="D") == 0)
                {
                    var faktura = new Fakturi
                                      {
                                          Broj = fd1.BROJ,
                                          Godina = fd1.GODINA,
                                          Neto = (decimal?) fd1.NETO,
                                          DanocnaOsnovica = (decimal?) fd1.DANOSN,
                                          Personalen_Stapka = (decimal?) fd1.PERS_STAPKA1,
                                          PersonalenDanok = (decimal?) fd1.PERSDANOK1,
                                          Trosoci = (decimal?) fd1.TROSOCI,
                                          DDV_Osnovica = (decimal?) fd1.DDV_OSN,
                                          DDV_Iznos = (decimal?) fd1.DDV_IZN,
                                          DDV_Stapka = (decimal?) fd1.DDV_STAPKA,
                                          Iznos = (decimal?) fd1.IZNOS,
                                          Tekst = fd1.TEKST,
                                          DOG_K = fd1.DOG_K,
                                          GOD_K = fd1.GOD_K,
                                          Plateno = (decimal?) fd1.PLATENO,
                                          DatumPlateno = fd1.DATUMP,
                                          GL_Spisok = fd1.GL_SPISOK,
                                          GOD_S = fd1.GOD_S,
                                          prazno = "D",
                                          tempf = (decimal?) fd1.tempf,
                                          Seuste_Ne = fd1.SEUSTE_NE
                                      };
                    DateTime dateTime;
                    if (DateTime.TryParse(fd1.DATUMF, out dateTime))
                        if (dateTime >= Convert.ToDateTime("1753/1/1") && dateTime <= DateTime.MaxValue)
                            faktura.Datum = dateTime;

                    _avtorskaEntities.AddToFakturi(faktura);
                    _avtorskaEntities.SaveChanges();
                }
            }
            Cursor.Current = Cursors.Default;
            MessageBox.Show("FAKTURI_D => Fakturi : " + i);
        }

        private void buttonFakturiDogovorNaDeloFAK_DOG_D_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            int i = 0;
            foreach (var fakDogD in _mladinskaEntities.FAK_DOG_D)
            {
                var fd = fakDogD;
                var faktura = _avtorskaEntities.Fakturi
                    .FirstOrDefault(f => f.Broj == fd.BROJ_F && f.Godina == fd.GODINA_F && f.prazno == "d");
                var dogovorNaDelo =
                    _avtorskaEntities.DogovorNaDelo.FirstOrDefault(d => d.Broj == fd.BROJ_A && d.Godina == fd.GODINA_A);
                if (dogovorNaDelo == null || faktura == null) continue;
                var fakturiDogovorNaDelo = new FakturiDogovorNaDelo {DogovorNaDelo = dogovorNaDelo, Fakturi = faktura};
                _avtorskaEntities.AddToFakturiDogovorNaDelo(fakturiDogovorNaDelo);
                _avtorskaEntities.SaveChanges();
            }
            Cursor.Current = Cursors.Default;
            MessageBox.Show("FAK_DOG_D => fakturiDogovorNaDelo : " + i);
        }

        private void buttonAvtorskiDogovorDOG_a_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            int i = 0;
            foreach (var doga in _mladinskaEntities.DOG_A)
            {
                var dogd1 = doga;
                int c =
                    _avtorskaEntities.AvtorskiDogovor.Count(d => d.BrojDogovor == dogd1.BROJ && d.Godina == dogd1.GODINA);
                if (_avtorskaEntities.AvtorskiDogovor.Count(d => d.BrojDogovor == dogd1.BROJ && d.Godina == dogd1.GODINA) != 0)
                    continue;
                var avtorski = new AvtorskiDogovor
                                   {
                                       BrojDogovor = dogd1.BROJ,
                                       Kraj = dogd1.KRAJ,
                                       TipA = dogd1.TIPA,
                                       God_F = dogd1.GOD_F,
                                       Dog_F = dogd1.DOG_F,
                                       Godina = dogd1.GODINA,
                                       OpisClen2 = dogd1.OPISCLEN2,
                                       OpisRabota = dogd1.OPISRABOTA,
                                       prazno = dogd1.prazno,
                                       Bruto = (decimal?) dogd1.BRUTO,
                                       Neto = (decimal?) dogd1.NETO,
                                       Personalen = (decimal?) dogd1.PERSONALEN,
                                       Popust = (decimal?) dogd1.POPUST,
                                       Provizija = (decimal?) dogd1.PROVIZIJA,
                                       tempf = (decimal?) dogd1.tempf,
                                       Mnozi = dogd1.MNOZI,
                                       Sobiraj = dogd1.SOBIRAJ,
                                       Rabotnik1 = dogd1.RABOTNIK1,
                                       Dejnosti =
                                           _avtorskaEntities.Dejnosti
                                           .FirstOrDefault(d => d.PopustDejnost == dogd1.POPUST)
                                   };
                DateTime dateTime;
                if (DateTime.TryParse(dogd1.DATUM, out dateTime))
                    if (dateTime >= Convert.ToDateTime("1753/1/1") && dateTime <= DateTime.MaxValue)
                        avtorski.Datum = dateTime;
                
                DateTime datDo;
                if (DateTime.TryParse(dogd1.DAT_DO, out datDo))
                    if (datDo>= Convert.ToDateTime("1753/1/1") && datDo <= DateTime.MaxValue)
                        avtorski.Dat_Do = datDo;

                DateTime datOd;
                if (DateTime.TryParse(dogd1.DAT_OD, out datOd))
                    if (datOd >= Convert.ToDateTime("1753/1/1") && datOd <= DateTime.MaxValue)
                        avtorski.Dat_Od = datOd;

                var idf = dogd1.DOG_F ?? -1;
                string sdf = idf.ToString();
                if (idf != -1) avtorski.Firma = _avtorskaEntities.Firma.FirstOrDefault(f => f.BrFirma.Equals(sdf));

                _avtorskaEntities.AddToAvtorskiDogovor(avtorski);
                _avtorskaEntities.SaveChanges();
            }
            Cursor.Current = Cursors.Default;
            MessageBox.Show("DOG_A => AvtorskiDogovor : " + i);
        }

        private void buttonFakturiFAKTURI_A_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            int i = 0;
            foreach (var fd in _mladinskaEntities.FAKTURI_A)
            {
                var fd1 = fd;
                if (_avtorskaEntities.Fakturi.Count(d => d.Broj == fd1.BROJ && d.Godina == fd1.GODINA && fd1.prazno == "A") == 0)
                {
                    var faktura = new Fakturi
                                      {
                                          Broj = fd1.BROJ,
                                          Godina = fd1.GODINA,
                                          Neto = (decimal?) fd1.NETO,
                                          DanocnaOsnovica = (decimal?) fd1.DANOSN,
                                          Trosoci = (decimal?) fd1.TROSOCI,
                                          DDV_Osnovica = (decimal?) fd1.DDV_OSN,
                                          DDV_Iznos = (decimal?) fd1.DDV_IZN,
                                          DDV_Stapka = (decimal?) fd1.DDV_STAPKA,
                                          Iznos = (decimal?) fd1.IZNOS,
                                          PersonalenDanok = (decimal?) fd1.PERSDANOK,
                                          Personalen_Stapka = (decimal?) fd1.PERS_STAPKA,
                                          Popust = (decimal?) fd1.POPUST,
                                          Tekst = fd1.TEKST,
                                          DOG_K = fd1.DOG_K,
                                          GOD_K = fd1.GOD_K,
                                          Plateno = (decimal?) fd1.PLATENO,
                                          DatumPlateno = fd1.DATUMP,
                                          GL_Spisok = fd1.GL_SPISOK,
                                          GOD_S = fd1.GOD_S,
                                          prazno = "A",
                                          tempf = (decimal?) fd1.tempf,
                                          Seuste_Ne = fd1.SEUSTE_NE
                                      };
                    DateTime dateTime;
                    if (DateTime.TryParse(fd1.DATUMF, out dateTime))
                        if (dateTime >= Convert.ToDateTime("1753/1/1") && dateTime <= DateTime.MaxValue)
                            faktura.Datum = dateTime;

                    _avtorskaEntities.AddToFakturi(faktura);
                    _avtorskaEntities.SaveChanges();
                }
            }
            Cursor.Current = Cursors.Default;
            MessageBox.Show("FAKTURI_A => Fakturi : " + i);
        }

        private void buttonFakturiAvtorskiDogovorFAK_DOG_A_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            int i = 0;
            foreach (var fakDogA in _mladinskaEntities.FAK_DOG_A)
            {
                var fa = fakDogA;
                var faktura = _avtorskaEntities.Fakturi
                    .FirstOrDefault(f => f.Broj == fa.BROJ_F && f.Godina == fa.GODINA_F && f.prazno == "A");
                var avtorskiDogovor =
                    _avtorskaEntities.AvtorskiDogovor.FirstOrDefault(d => d.BrojDogovor == fa.BROJ_A && d.Godina == fa.GODINA_A);
                if (avtorskiDogovor == null || faktura == null) continue;
                var fakturiAvtorskiDogovor = new FakturiAvtorskiDogovor
                                                 {
                                                     AvtorskiDogovor = avtorskiDogovor,
                                                     Fakturi = faktura,
                                                     Bruto = avtorskiDogovor.Bruto
                                                 };
                _avtorskaEntities.AddToFakturiAvtorskiDogovor(fakturiAvtorskiDogovor);
                _avtorskaEntities.SaveChanges();
            }
            Cursor.Current = Cursors.Default;
            MessageBox.Show("FAK_DOG_A => fakturiAvtorskiDogovor : " + i);
        }

        private void buttonDogovorSoAvtorDOG_A_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            int i = 0;
            foreach (var dogA in _mladinskaEntities.DOG_A)
            {
                DOG_A a = dogA;
                var avtorskiDogovor =
                    _avtorskaEntities.AvtorskiDogovor.FirstOrDefault(d => d.BrojDogovor == a.BROJ && d.Godina == a.GODINA);
                var avtorskiClenovi = _avtorskaEntities.AvtorskiClenovi
                    .FirstOrDefault(f => f.ClenskiBroj == a.RABOTNIK1);

                if (avtorskiDogovor == null || avtorskiClenovi == null) continue;
                var dogovorSoAvtor = new DogovorSoAvtor {AvtorskiClenovi = avtorskiClenovi,AvtorskiDogovor = avtorskiDogovor};
                _avtorskaEntities.AddToDogovorSoAvtor(dogovorSoAvtor);
                _avtorskaEntities.SaveChanges();
            }
            Cursor.Current = Cursors.Default;
            MessageBox.Show("DOG_A => dogovorSoAvtor : " + i);
        }
    }
}
