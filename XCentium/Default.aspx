<%@ Page Title="Home Page" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="XCentium._Default" %>

<asp:Content runat="server" ID="FeaturedContent" ContentPlaceHolderID="FeaturedContent">
    <div class="input-controls">
        <label>URL to scrape for Images, Word Count, and Word Rank</label>
        <asp:TextBox runat="server" ID="urlInput"></asp:TextBox>
        <asp:Button OnClick="ScrapeUrl" Text="Scrape" runat="server" />
    </div>
    <div class="output-controls">
        <label class="error" id="outputError" visible="false" runat="server"></label>
        <asp:Panel ID="carouselPanel" Visible="false" runat="server">
            <div id="myCarousel" class="carousel slide" data-ride="carousel">
                <!-- Indicators -->
                <asp:Repeater ID="indicators" runat="server">
                    <HeaderTemplate>
                        <ol class="carousel-indicators">
                    </HeaderTemplate>
                    <ItemTemplate>
                        <li data-target="#myCarousel" data-slide-to="<%# Container.ItemIndex %>" class="<%# GetItemClass(Container.ItemIndex) %>"></li>
                    </ItemTemplate>
                    <FooterTemplate>
                        </ol>
                    </FooterTemplate>
                </asp:Repeater>
                <!-- Wrapper for slides -->
                <div class="carousel-inner" role="listbox">
                    <asp:Repeater ID="images" runat="server">
                        <ItemTemplate>
                            <div class="item <%# GetItemClass(Container.ItemIndex) %>">
                                <img src='<%#Eval("src") %>' alt='<%#Eval("alt") %>' class="img-responsive center-block" />
                            </div>
                        </ItemTemplate>
                    </asp:Repeater>
                </div>
                <!-- Left and right controls -->
                <a class="left carousel-control" href="#myCarousel" role="button" data-slide="prev">
                    <span class="glyphicon glyphicon-chevron-left" aria-hidden="true"></span>
                    <span class="sr-only">Previous</span>
                </a>
                <a class="right carousel-control" href="#myCarousel" role="button" data-slide="next">
                    <span class="glyphicon glyphicon-chevron-right" aria-hidden="true"></span>
                    <span class="sr-only">Next</span>
                </a>
            </div>
        </asp:Panel>
        <label class="output-list" id="outputWordList" visible="false" runat="server"></label>
        <asp:PlaceHolder ID="phResultsTable" runat="server"></asp:PlaceHolder>
    </div>
</asp:Content>
