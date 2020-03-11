namespace ServiceIntegration {
    class CallModel {
        public CallModel() { }

        public CallModel(string url_ligacao, string origem_tel, string destino_tel, string dt_inicio_chamada, string dt_fim_chamada,
            string tempo_conversacao, string domain, string company_id, string calltype, string agent) {
            UrlLigacao = url_ligacao;
            OrigemTel = origem_tel;
            DestinoTel = destino_tel;
            DtInicioChamada = dt_inicio_chamada;
            DtFimChamada = dt_fim_chamada;
            TempoConversacao = tempo_conversacao;
            Domain = Domain;
            CompanyId = company_id;
            TipoChamada = calltype;
            CallName = agent;
        }

        public string CompanyId { get; set; }

        public string Domain { get; set; }

        public string UrlLigacao { get; set; }

        public string CallName { get; set; }

        public string TipoChamada { get; set; }

        public string OrigemTel { get; set; }

        public string DestinoTel { get; set; }

        public string DtInicioChamada { get; set; }

        public string DtFimChamada { get; set; }

        public string TempoConversacao { get; set; }
    }
}
