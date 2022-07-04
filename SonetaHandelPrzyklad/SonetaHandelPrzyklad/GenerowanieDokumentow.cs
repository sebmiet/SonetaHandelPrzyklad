using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Soneta.Business;
using Soneta.Towary;
using Soneta.Handel;
using Soneta.CRM;
using Soneta.Types;
using Soneta.Magazyny;
using SonetaHandelPrzyklad;

[assembly: Worker(typeof(GenerowanieDokumentow), typeof(DokHandlowe))]


namespace SonetaHandelPrzyklad
{
    class GenerowanieDokumentow
    {
        [Context]
        public Session Session { get; set; }
        [Action("Generuj ZK", Mode = ActionMode.SingleSession | ActionMode.ConfirmSave | ActionMode.Progress, Target = ActionTarget.ToolbarWithText)]
        public void GenerujFakture()
        {
            using(Session session = Session.Login.CreateSession(false, false))
            {
                HandelModule handelModule = HandelModule.GetInstance(session);
                TowaryModule towaryModule = TowaryModule.GetInstance(session);
                MagazynyModule magazynyModule = MagazynyModule.GetInstance(session);
                CRMModule crmModule = CRMModule.GetInstance(session);

                using(ITransaction transaction = session.Logout(true))
                {
                    DokumentHandlowy dokument = new DokumentHandlowy();
                    DefDokHandlowego defDokumentu = handelModule.DefDokHandlowych.WgSymbolu["ZK"];
                    dokument.Definicja = defDokumentu;
                    dokument.Magazyn = magazynyModule.Magazyny.Firma;

                    handelModule.DokHandlowe.AddRow(dokument);

                    Kontrahent kontrahent = crmModule.Kontrahenci.WgKodu["Abc"];

                    Towary towary = towaryModule.Towary;

                    using(ITransaction transDokPos = session.Logout(true))
                    {
                        foreach (var towar in towary.WgNazwy)
                        {
                            Random rd = new Random();
                            int rand = rd.Next(1, 10);
                            PozycjaDokHandlowego pozycja = new PozycjaDokHandlowego(dokument);
                            handelModule.PozycjeDokHan.AddRow(pozycja);
                            pozycja.Towar = towar;
                            pozycja.Ilosc = new Quantity(rand, null);
                            pozycja.Cena = towar.CenaZakupuKartotekowa;
                        }
                        transDokPos.CommitUI();
                    }

                    transaction.Commit();
                }

                session.Save();
            }
        }
    }
}
