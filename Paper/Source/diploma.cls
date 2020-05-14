%%
%% This is file `diploma.cls',
%% generated with the docstrip utility.
%%
%% The original source files were:
%%
%% VersionInfo.dtx 
%% Introduction.dtx 
%% UserManual.dtx 
%% ImplementationBeginning.dtx 
%% Constants.dtx 
%% VariablesDeclaration.dtx 
%% BaseClassAndPackages.dtx 
%% OptionsDeclaration.dtx 
%% PublicMacros.dtx 
%% InternalMacros.dtx 
%% SourceCodeListings.dtx 
%% ImplementationFinale.dtx 
%% 
%% Original was created by doc. Mgr. Jiří Dvorský, Ph.D., available at http://www.cs.vsb.cz/dvorsky/Download/LaTeX/Diploma.zip
%% 
%% Toto dílo podléhá licenci CC BY-NC-ND 3.0 CZ, https://creativecommons.org/licenses/by-nc-nd/3.0/cz/
%% Můžete jej šířit a používat pro nekomerční účely, musí však být uveden autor a dokument nelze měnit.
%% 
%% This work is licensed under CC BY-NC-ND 3.0 CZ, https://creativecommons.org/licenses/by-nc-nd/3.0/cz/deed.en
%% You may distribute it and use it for non-commercial purposes, but the author must be mentioned and the document can not be changed.
%% 
\def\fileversion{4.3}
\def\filedate{2018/05/30}
\def\docdate{30. května 2019}
\NeedsTeXFormat{LaTeX2e}
\ProvidesClass{diploma}[\filedate\space \fileversion\space diploma LaTeX document class]
\RequirePackage{ifthen}
\RequirePackage[utf8]{inputenc}
\newcommand{\Dipl@BachelorThesis}{0}
\newcommand{\Dipl@BachelorPractice}{1}
\newcommand{\Dipl@MasterThesis}{2}
\newcommand{\Dipl@PhDThesis}{3}
\newcommand{\Dipl@CzechLanguage}{0}
\newcommand{\Dipl@EnglishLanguage}{1}
\newcommand{\Dipl@SlovakLanguage}{2}
\newcommand{\Dipl@CzechBachelorMaleAuthorDeclaration}{Prohlašuji, že jsem tuto bakalářskou práci vypracoval samostatně. Uvedl jsem všechny literární prameny a publikace, ze kterých jsem čerpal.}
\newcommand{\Dipl@CzechBachelorFemaleAuthorDeclaration}{Prohlašuji, že jsem tuto bakalářskou práci vypracovala samostatně. Uvedla jsem všechny literární prameny a publikace, ze kterých jsem čerpala.}
\newcommand{\Dipl@CzechMasterMaleAuthorDeclaration}{Prohlašuji, že jsem tuto diplomovou práci vypracoval samostatně. Uvedl jsem všechny literární prameny a publikace, ze kterých jsem čerpal.}
\newcommand{\Dipl@CzechMasterFemaleAuthorDeclaration}{Prohlašuji, že jsem tuto diplomovou práci vypracovala samostatně. Uvedla jsem všechny literární prameny a publikace, ze kterých jsem čerpala.}
\newcommand{\Dipl@CzechPhDMaleAuthorDeclaration}{Prohlašuji, že jsem tuto disertační práci vypracoval samostatně. Uvedl jsem všechny literární prameny a publikace, ze kterých jsem čerpal.}
\newcommand{\Dipl@CzechPhDFemaleAuthorDeclaration}{Prohlašuji, že jsem tuto disertační práci vypracovala samostatně. Uvedla jsem všechny literární prameny a publikace, ze kterých jsem čerpala.}
\newcommand{\Dipl@SlovakBachelorMaleAuthorDeclaration}{Prehlasujem, že som túto bakalársku prácu vypracoval samostatne. Uviedol som všetky literárne pramene a publikácie, z ktorých som čerpal.}
\newcommand{\Dipl@SlovakBachelorFemaleAuthorDeclaration}{Prehlasujem, že som túto bakalársku prácu vypracovala samostatne. Uviedla som všetky literárne pramene a publikácie, z ktorých som čerpala.}
\newcommand{\Dipl@SlovakMasterMaleAuthorDeclaration}{Prehlasujem, že som túto diplomovú prácu vypracoval samostatne. Uviedol som všetky literárne pramene a publikácie, z ktorých som čerpal.}
\newcommand{\Dipl@SlovakMasterFemaleAuthorDeclaration}{Prehlasujem, že som túto diplomovú prácu vypracovala samostatne. Uviedla som všetky literárne pramene a publikácie, z ktorých som čerpala.}
\newcommand{\Dipl@SlovakPhDMaleAuthorDeclaration}{Prehlasujem, že som túto disertačnú prácu vypracoval samostatne. Uviedol som všetky literárne pramene a publikácie, z ktorých som čerpal.}
\newcommand{\Dipl@SlovakPhDFemaleAuthorDeclaration}{Prehlasujem, že som túto disertačnú prácu vypracovala samostatne. Uviedla som všetky literárne pramene a publikácie, z ktorých som čerpala.}
\newcommand{\Dipl@EnglishBachelorAuthorDeclaration}{I hereby declare that this bachelor's thesis was written by myself. I have quoted all the references I have drawn upon.}
\newcommand{\Dipl@EnglishMasterAuthorDeclaration}{I hereby declare that this master's thesis was written by myself. I have quoted all the references I have drawn upon.}
\newcommand{\Dipl@EnglishPhDAuthorDeclaration}{I hereby declare that this PhD thesis was written by myself. I have quoted all the references I have drawn upon.}
\newtoks\Dipl@UniversityName
\Dipl@UniversityName={No university name is given}
\newtoks\Dipl@FacultyName
\Dipl@FacultyName={No faculty name is given}
\newtoks\Dipl@DepartmentName
\Dipl@DepartmentName={No department name is given}
\newtoks\Dipl@ThesisAuthor
\Dipl@ThesisAuthor={No author name is given}
\newtoks\Dipl@CzechThesisTitle
% EDIT START
\Dipl@CzechThesisTitle={}
% EDIT END
\newtoks\Dipl@EnglishThesisTitle
\Dipl@EnglishThesisTitle={No English thesis title is given}
\newtoks\Dipl@SubmissionDate
\Dipl@SubmissionDate={No submission date is given}
\newtoks\Dipl@ThesisAssignmentImagePath
\Dipl@ThesisAssignmentImagePath={}
\newtoks\Dipl@AuthorDeclaration
\Dipl@AuthorDeclaration={}
\newtoks\Dipl@AuthorDeclarationImageFile
\Dipl@AuthorDeclarationImageFile={}
\newtoks\Dipl@CooperatingPersonsDeclaration
\Dipl@CooperatingPersonsDeclaration={}
\newtoks\Dipl@CooperatingPersonsDeclarationImageFile
\Dipl@CooperatingPersonsDeclarationImageFile={}
\newtoks\Dipl@Thanks
\Dipl@Thanks={}
\newtoks\Dipl@CzechAbstract
% EDIT START
\Dipl@CzechAbstract={}
% EDIT END
\newtoks\Dipl@CzechKeywords
\Dipl@CzechKeywords={No Czech or Slovak keywords are given}
\newtoks\Dipl@EnglishAbstract
\Dipl@EnglishAbstract={No English abstract is given}
\newtoks\Dipl@EnglishKeywords
\Dipl@EnglishKeywords={No English keywords are given}
\newtoks\Dipl@Acronyms
\Dipl@Acronyms={}
\newtoks\Dipl@DefaultSourceCodeLanguage
\Dipl@DefaultSourceCodeLanguage={csharp}
\newtoks\Dipl@BabelLanguageOptions
\newboolean{Dipl@AuthorIsMale}
\setboolean{Dipl@AuthorIsMale}{true}
\newboolean{Dipl@PrintCooperatingPersonsDeclaration}
\setboolean{Dipl@PrintCooperatingPersonsDeclaration}{false}
\newboolean{Dipl@IsAppendix}
\setboolean{Dipl@IsAppendix}{false}
\newcounter{Dipl@CurrentThesisType}
\setcounter{Dipl@CurrentThesisType}{\Dipl@MasterThesis}
\newcounter{Dipl@CurrentThesisLanguage}
\setcounter{Dipl@CurrentThesisLanguage}{\Dipl@CzechLanguage}
\newcounter{Dipl@ThesisAssignmentImageCounter}
\LoadClass[11pt]{article}
\RequirePackage[T1]{fontenc}
\RequirePackage{lmodern}
\RequirePackage[a4paper,includefoot,nohead,marginparwidth=0mm,marginparsep=0mm,left=30mm,right=20mm,top=30mm,bottom=30mm]{geometry}
\RequirePackage{booktabs}
\RequirePackage{graphicx}
\RequirePackage[final]{listings}
\RequirePackage{longtable}
\RequirePackage{caption}
\RequirePackage{rotating}
\RequirePackage{setspace}
\RequirePackage{theorem}
\RequirePackage{titlesec}
\RequirePackage[nottoc,notindex,section]{tocbibind}
\RequirePackage{totcount}
\RequirePackage{xcolor}
\RequirePackage[a-1b]{pdfx}
\DeclareOption{czech}
{
\setcounter{Dipl@CurrentThesisLanguage}{\Dipl@CzechLanguage}
\Dipl@BabelLanguageOptions={english,main=czech}
\Dipl@UniversityName={VŠB -- Technická univerzita Ostrava}
\Dipl@FacultyName={Fakulta elektrotechniky a informatiky}
}
\DeclareOption{english}
{
\setcounter{Dipl@CurrentThesisLanguage}{\Dipl@EnglishLanguage}
\Dipl@BabelLanguageOptions{czech,main=english}
\Dipl@UniversityName={VŠB -- Technical University of Ostrava}
\Dipl@FacultyName={Faculty of Electrical Engineering and Computer Science}
}
\DeclareOption{slovak}
{
\setcounter{Dipl@CurrentThesisLanguage}{\Dipl@SlovakLanguage}
\Dipl@BabelLanguageOptions={english,main=slovak}
\Dipl@UniversityName={VŠB -- Technická univerzita Ostrava}
\Dipl@FacultyName={Fakulta elektrotechniky a informatiky}
}
\DeclareOption{bachelor}
{
\setcounter{Dipl@CurrentThesisType}{\Dipl@BachelorThesis}
\Dipl@CooperatingPersonsDeclaration={\Dipl@LanguageDependentText{Souhlasím se zveřejněním této bakalářské práce dle požadavků čl.\ 26, odst.\ 9 Studijního a~zkušebního řádu pro studium v bakalářských programech VŠB-TU Ostrava.}{Súhlasím so zverejnením tejto bakalárskej práce podľa požiadaviek čl .\ 26, odst.\ 9 Študijného a~skúšobného poriadku pre štúdium v bakalárskych programoch  VŠB-TU Ostrava.}{I hereby agree to the publishing of the bachelor's thesis as per s. 26, ss. 9 of the Study and Examination Regulations for Bachelor's Degree Programmes at VŠB -- Technical University of Ostrava.}}
}
\DeclareOption{bachelorpractice}
{
\setcounter{Dipl@CurrentThesisType}{\Dipl@BachelorPractice}
\Dipl@CzechThesisTitle={Absolvování individuální odborné praxe}
\Dipl@EnglishThesisTitle={Individual Professional Practice in the Company}
\Dipl@CooperatingPersonsDeclaration={\Dipl@LanguageDependentText{Souhlasím se zveřejněním této bakalářské práce dle požadavků čl.\ 26, odst.\ 9 Studijního a~zkušebního řádu pro studium v bakalářských programech VŠB-TU Ostrava.}{Súhlasím so zverejnením tejto bakalárskej práce podľa požiadaviek čl .\ 26, odst.\ 9 Študijného a~skúšobného poriadku pre štúdium v bakalárskych programoch  VŠB-TU Ostrava.}{I hereby agree to the publishing of the bachelor's thesis as per s. 26, ss. 9 of the Study and Examination Regulations for Bachelor's Degree Programmes at VŠB -- Technical University of Ostrava.}}
}
\DeclareOption{master}
{
\setcounter{Dipl@CurrentThesisType}{\Dipl@MasterThesis}
\Dipl@CooperatingPersonsDeclaration={\Dipl@LanguageDependentText{Souhlasím se zveřejněním této diplomové práce dle požadavků čl.\ 26, odst.\ 9 Studijního a~zkušebního řádu pro studium v magisterských programech VŠB-TU Ostrava.}{Súhlasím so zverejnením tejto diplomovej práce podľa požiadaviek čl .\ 26, odst.\ 9 Študijného a~skúšobného poriadku pre štúdium v bakalárskych programoch  VŠB-TU Ostrava.}{I hereby agree to the publishing of the master's thesis as per s. 26, ss. 9 of the Study and Examination Regulations for Master's Degree Programmes at VŠB -- Technical University of Ostrava.}}
}
\DeclareOption{phd}{\setcounter{Dipl@CurrentThesisType}{\Dipl@PhDThesis}}
\DeclareOption{cpdeclaration}{\setboolean{Dipl@PrintCooperatingPersonsDeclaration}{true}}
\DeclareOption{dept410}{\Dipl@DepartmentName={\Dipl@LanguageDependentText{Katedra elektroenergetiky}{Katedra elektroenergetiky}{Department of Electrical Power Engineering}}}
\DeclareOption{dept420}{\Dipl@DepartmentName={\Dipl@LanguageDependentText{Katedra elektrotechniky}{Katedra elektrotechniky}{Department of Electrical Engineering}}}
\DeclareOption{dept430}{\Dipl@DepartmentName={\Dipl@LanguageDependentText{Katedra elektroniky}{Katedra elektroniky}{Department of Electronics}}}
\DeclareOption{dept440}{\Dipl@DepartmentName={\Dipl@LanguageDependentText{Katedra telekomunikační techniky}{Katedra telekomunikační techniky}{Department of Telecommunications}}}
\DeclareOption{dept450}{\Dipl@DepartmentName={\Dipl@LanguageDependentText{Katedra kybernetiky a biomedicínského inženýrství}{Katedra kybernetiky a biomedicínského inženýrství}{Department of Cybernetics and Biomedical Engineering}}}
\DeclareOption{dept460}{\Dipl@DepartmentName={\Dipl@LanguageDependentText{Katedra informatiky}{Katedra informatiky}{Department of Computer Science}}}
\DeclareOption{dept470}{\Dipl@DepartmentName={\Dipl@LanguageDependentText{Katedra aplikované matematiky}{Katedra aplikované matematiky}{Department of Applied Mathematics}}}
\DeclareOption{dept480}{\Dipl@DepartmentName={\Dipl@LanguageDependentText{Katedra fyziky}{Katedra fyziky}{Department of Physics}}}
\DeclareOption{male}
{
\setboolean{Dipl@AuthorIsMale}{true}
\ifthenelse{\(\value{Dipl@CurrentThesisType}=\Dipl@BachelorThesis\) \or \(\value{Dipl@CurrentThesisType}=\Dipl@BachelorPractice\)}
{
\Dipl@AuthorDeclaration={\Dipl@LanguageDependentText{\Dipl@CzechBachelorMaleAuthorDeclaration}{\Dipl@SlovakBachelorMaleAuthorDeclaration}{\Dipl@EnglishBachelorAuthorDeclaration}}
}
{\relax}
\ifthenelse{\value{Dipl@CurrentThesisType}=\Dipl@MasterThesis}
{
\Dipl@AuthorDeclaration={\Dipl@LanguageDependentText{\Dipl@CzechMasterMaleAuthorDeclaration}{\Dipl@SlovakMasterMaleAuthorDeclaration}{\Dipl@EnglishMasterAuthorDeclaration}}
}
{\relax}
\ifthenelse{\value{Dipl@CurrentThesisType}=\Dipl@PhDThesis}
{
\Dipl@AuthorDeclaration={\Dipl@LanguageDependentText{\Dipl@CzechPhDMaleAuthorDeclaration}{\Dipl@SlovakPhDMaleAuthorDeclaration}{\Dipl@EnglishPhDAuthorDeclaration}}
}
{\relax}
}
\DeclareOption{female}
{
\setboolean{Dipl@AuthorIsMale}{false}
\ifthenelse{\(\value{Dipl@CurrentThesisType}=\Dipl@BachelorThesis\) \or \(\value{Dipl@CurrentThesisType}=\Dipl@BachelorPractice\)}
{
\Dipl@AuthorDeclaration={\Dipl@LanguageDependentText{\Dipl@CzechBachelorFemaleAuthorDeclaration}{\Dipl@SlovakBachelorFemaleAuthorDeclaration}{\Dipl@EnglishBachelorAuthorDeclaration}}
}
{\relax}
\ifthenelse{\value{Dipl@CurrentThesisType}=\Dipl@MasterThesis}
{
\Dipl@AuthorDeclaration={\Dipl@LanguageDependentText{\Dipl@CzechMasterFemaleAuthorDeclaration}{\Dipl@SlovakMasterFemaleAuthorDeclaration}{\Dipl@EnglishMasterAuthorDeclaration}}
}
{\relax}
\ifthenelse{\value{Dipl@CurrentThesisType}=\Dipl@PhDThesis}
{
\Dipl@AuthorDeclaration={\Dipl@LanguageDependentText{\Dipl@CzechPhDFemaleAuthorDeclaration}{\Dipl@SlovakPhDFemaleAuthorDeclaration}{\Dipl@EnglishPhDAuthorDeclaration}}
}
{\relax}
}
\DeclareOption{ansic}{\Dipl@DefaultSourceCodeLanguage={ansic}}
\DeclareOption{bash}{\Dipl@DefaultSourceCodeLanguage={bash}}
\DeclareOption{cil}{\Dipl@DefaultSourceCodeLanguage={cil}}
\DeclareOption{cpp}{\Dipl@DefaultSourceCodeLanguage={cpp}}
\DeclareOption{csharp}{\Dipl@DefaultSourceCodeLanguage={csharp}}
\DeclareOption{delphi}{\Dipl@DefaultSourceCodeLanguage={delphi}}
\DeclareOption{gnuplot}{\Dipl@DefaultSourceCodeLanguage={gnuplot}}
\DeclareOption{html}{\Dipl@DefaultSourceCodeLanguage={html}}
\DeclareOption{java}{\Dipl@DefaultSourceCodeLanguage={java}}
\DeclareOption{latex}{\Dipl@DefaultSourceCodeLanguage={latex}}
\DeclareOption{lisp}{\Dipl@DefaultSourceCodeLanguage={lisp}}
\DeclareOption{lua}{\Dipl@DefaultSourceCodeLanguage={lua}}
\DeclareOption{make}{\Dipl@DefaultSourceCodeLanguage={make}}
\DeclareOption{mathematica}{\Dipl@DefaultSourceCodeLanguage={mathematica}}
\DeclareOption{matlab}{\Dipl@DefaultSourceCodeLanguage={matlab}}
\DeclareOption{octave}{\Dipl@DefaultSourceCodeLanguage={octave}}
\DeclareOption{pascal}{\Dipl@DefaultSourceCodeLanguage={pascal}}
\DeclareOption{perl}{\Dipl@DefaultSourceCodeLanguage={perl}}
\DeclareOption{php}{\Dipl@DefaultSourceCodeLanguage={php}}
\DeclareOption{pov}{\Dipl@DefaultSourceCodeLanguage={pov}}
\DeclareOption{prolog}{\Dipl@DefaultSourceCodeLanguage={prolog}}
\DeclareOption{python}{\Dipl@DefaultSourceCodeLanguage={python}}
\DeclareOption{ruby}{\Dipl@DefaultSourceCodeLanguage={ruby}}
\DeclareOption{r}{\Dipl@DefaultSourceCodeLanguage={r}}
\DeclareOption{sh}{\Dipl@DefaultSourceCodeLanguage={sh}}
\DeclareOption{sql}{\Dipl@DefaultSourceCodeLanguage={sql}}
\DeclareOption{tcltk}{\Dipl@DefaultSourceCodeLanguage={tcltk}}
\DeclareOption{vb}{\Dipl@DefaultSourceCodeLanguage={vb}}
\DeclareOption{vhdl}{\Dipl@DefaultSourceCodeLanguage={vhdl}}
\DeclareOption{xml}{\Dipl@DefaultSourceCodeLanguage={xml}}
\DeclareOption{xslt}{\Dipl@DefaultSourceCodeLanguage={xslt}}
\DeclareOption*{\PassOptionsToClass{\CurrentOption}{article}}
\ProcessOptions
\RequirePackage[\the\Dipl@BabelLanguageOptions]{babel}
\newcommand{\ThesisAuthor}[1]{\Dipl@ThesisAuthor={#1}}
\newcommand{\CzechThesisTitle}[1]{\ifthenelse{\value{Dipl@CurrentThesisType}=\Dipl@BachelorPractice}{\relax}{\Dipl@CzechThesisTitle={#1}}}
\newcommand{\EnglishThesisTitle}[1]{\ifthenelse{\value{Dipl@CurrentThesisType}=\Dipl@BachelorPractice}{\relax}{\Dipl@EnglishThesisTitle={#1}}}
\newcommand{\SubmissionDate}[1]{\Dipl@SubmissionDate={#1}}
\newcommand{\ThesisAssignmentImagePath}[1]{\Dipl@ThesisAssignmentImagePath={#1}}
\newcommand{\AuthorDeclarationImageFile}[1]{\Dipl@AuthorDeclarationImageFile={#1}}
\newcommand{\CooperatingPersonsDeclarationImageFile}[1]{\Dipl@CooperatingPersonsDeclarationImageFile={#1}}
\newcommand{\Thanks}[1]{\Dipl@Thanks={#1}}
\newcommand{\CzechAbstract}[1]{\Dipl@CzechAbstract={#1}}
\newcommand{\CzechKeywords}[1]{\Dipl@CzechKeywords={#1}}
\newcommand{\EnglishAbstract}[1]{\Dipl@EnglishAbstract={#1}}
\newcommand{\EnglishKeywords}[1]{\Dipl@EnglishKeywords={#1}}
\newcommand{\AddAcronym}[2]{\Dipl@AddTokens{\Dipl@Acronyms}{#1&--&#2\\}}
\newcommand{\MakeTitlePages}
{
\pagestyle{empty}
\Dipl@TitlePage
\Dipl@ThesisAssignmentPage
\Dipl@AuthorDeclarationPage
\Dipl@CooperatingPersonsDeclarationPage
% EDIT START
%\Dipl@WriteThanksPage
% EDIT END
\Dipl@AbstractsKeywordsPage
\pagestyle{plain}
\tableofcontents\cleardoublepage
\Dipl@WriteAcronymsPage
% EDIT START
\begingroup
\let\clearpage\relax
\listoffigures
\listoftables
\lstlistoflistings
\endgroup
\cleardoublepage
\relax
% EDIT END
}
{\theorembodyfont{\itshape}\newtheorem{definition}{\Dipl@LanguageDependentText{Definice}{Definícia}{Definition}}}
{\theorembodyfont{\itshape}\newtheorem{theorem}{\Dipl@LanguageDependentText{Věta}{Veta}{Theorem}}}
\newenvironment{proof}
{\par\noindent\textbf{\proofname}}
{\mbox{}\hspace*{5pt}\hfill\hbox{\vrule height5pt depth0pt width5pt}\par\medskip}
{\theorembodyfont{\rmfamily}\newtheorem{remark}{\Dipl@LanguageDependentText{Poznámka}{Poznámka}{Remark}}}
{\theoremstyle{break}\theorembodyfont{\rmfamily}\newtheorem{Dipl@examplebody}{\Dipl@LanguageDependentText{Příklad}{Príklad}{Example}}}
\newenvironment{example}
{\begin{Dipl@examplebody}}
{
\nopagebreak\mbox{}\hspace*{5pt}\hfill\hbox{\vrule height5pt depth0pt width5pt}
\end{Dipl@examplebody}
}
\newcommand{\InsertFigure}[4]
{
\begin{figure}
\centering
\includegraphics[width=#2]{#1}
\caption{#3}
\label{#4}
\end{figure}
}
\newcommand{\Dipl@TitlePage}
{
\begin{center}
{\Large\the\Dipl@UniversityName\par
\the\Dipl@FacultyName\par
\the\Dipl@DepartmentName\par}
\vspace{26mm}
{\Huge\bfseries\Dipl@LanguageDependentText{\the\Dipl@CzechThesisTitle}{\the\Dipl@CzechThesisTitle}{\the\Dipl@EnglishThesisTitle}\par}
\bigskip
{\Huge\bfseries\Dipl@LanguageDependentText{\the\Dipl@EnglishThesisTitle}{\the\Dipl@EnglishThesisTitle}{\the\Dipl@CzechThesisTitle}\par}
\end{center}
\vfill
{\Large\number\year\hfill\the\Dipl@ThesisAuthor}
\cleardoublepage
}
\newcommand{\Dipl@ThesisAssignmentPage}
{
\ifthenelse{\equal{\the\Dipl@ThesisAssignmentImagePath}{\empty}}
{
% EDIT START
%\begin{center}
%\Large\sffamily\bfseries
%\Dipl@LanguageDependentText{Tuto stránku nahradíte v tištěné verzi práce oficiálním zadáním Vaší diplomové či bakalářské práce.}{Tuto stránku nahradíte v tištěné verzi práce oficiálním zadáním Vaší diplomové či bakalářské práce.}{This page will be replaced by official thesis assignment in printed and electronic version of the submitted thesis.}
%\end{center}
% EDIT END
}
{
\setcounter{Dipl@ThesisAssignmentImageCounter}{1}
\whiledo{\value{Dipl@ThesisAssignmentImageCounter}>0}
{
\IfFileExists{\the\Dipl@ThesisAssignmentImagePath\theDipl@ThesisAssignmentImageCounter.jpg}
{
\Dipl@InsertFullPageImage{\the\Dipl@ThesisAssignmentImagePath\theDipl@ThesisAssignmentImageCounter.jpg}
\addtocounter{Dipl@ThesisAssignmentImageCounter}{1}
}
{
\setcounter{Dipl@ThesisAssignmentImageCounter}{-1}
}
}
% EDIT START
\cleardoublepage
}
% EDIT END
}
\newcommand{\Dipl@AuthorDeclarationPage}
{
\ifthenelse{\equal{\the\Dipl@AuthorDeclarationImageFile}{\empty}}
{
% EDIT START 
%\mbox{}
%\vfill
%\noindent
%\the\Dipl@AuthorDeclaration
%\par
%\vspace{15mm}
%\noindent\Dipl@LanguageDependentText{V Ostravě}{V Ostrave}{Ostrava,}~\the\Dipl@SubmissionDate\hfill\hbox to50mm{\dotfill}
% EDIT END
}
{
\Dipl@InsertFullPageImage{\the\Dipl@AuthorDeclarationImageFile}
% EDIT START
\cleardoublepage
}
% EDIT END
}
\newcommand{\Dipl@CooperatingPersonsDeclarationPage}
{
\ifthenelse{\boolean{Dipl@PrintCooperatingPersonsDeclaration}}
{
\ifthenelse{\equal{\the\Dipl@CooperatingPersonsDeclarationImageFile}{\empty}}
{
\mbox{}
\vfill
\noindent
\the\Dipl@CooperatingPersonsDeclaration
\par
\vspace{15mm}
\noindent\Dipl@LanguageDependentText{V Ostravě}{V Ostrave}{Ostrava,}~\the\Dipl@SubmissionDate\hfill\hbox to50mm{\dotfill}
}
{
\Dipl@InsertFullPageImage{\the\Dipl@CooperatingPersonsDeclarationImageFile}
}
\cleardoublepage
}
{\relax}
}
\newcommand{\Dipl@WriteThanksPage}
{
\ifthenelse{\equal{\the\Dipl@Thanks}{\empty}}
{\relax}
{
\mbox{}
\vfill
\noindent\the\Dipl@Thanks
\cleardoublepage
}
}
\newcommand{\Dipl@AbstractsKeywordsPage}
{
% EDIT START
\ifthenelse{\equal{\the\Dipl@CzechAbstract}{\empty}}
{
\noindent\the\Dipl@Thanks
}
{
\vspace*{20mm plus 0mm minus 25mm}
% EDIT END
\ifthenelse{\value{Dipl@CurrentThesisLanguage}=\Dipl@SlovakLanguage}
{
\begin{otherlanguage}{slovak}
\noindent\textbf{Abstrakt}
\medbreak
\noindent\the\Dipl@CzechAbstract
\bigbreak
\noindent\textbf{Kľúčové slová}:~\the\Dipl@CzechKeywords\par
 \end{otherlanguage}
}
{
\begin{otherlanguage}{czech}
\noindent\textbf{Abstrakt}
\medbreak
\noindent\the\Dipl@CzechAbstract
\bigbreak
\noindent\textbf{Klíčová slova}:~\the\Dipl@CzechKeywords\par
\end{otherlanguage}
}
\vspace{30mm plus 0mm minus 30mm}
% EDIT START
}
% EDIT END
\begin{otherlanguage}{english}
\noindent\textbf{Abstract}
\medbreak
\noindent\the\Dipl@EnglishAbstract
\bigbreak
\noindent\textbf{Keywords}:~\the\Dipl@EnglishKeywords\par
\end{otherlanguage}
\cleardoublepage
}
\newcommand{\Dipl@WriteAcronymsPage}
{
\ifthenelse{\equal{\the\Dipl@Acronyms}{\empty}}
{\relax}
{
\Dipl@LanguageDependentText
{
\section*{Seznam použitých zkratek a symbolů}
\addcontentsline{toc}{section}{Seznam použitých zkratek a symbolů}
}
{
\section*{Zoznam použitých skratiek a symbolov}
\addcontentsline{toc}{section}{Zoznam použitých skratiek a symbolov}
}
% EDIT START
{
\section*{List of Symbols and Abbreviations}
\addcontentsline{toc}{section}{List of Symbols and Abbreviations}
}
\begin{longtable*}[l]{p{0.1\textwidth}cp{0.8\textwidth}}
\the\Dipl@Acronyms
\end{longtable*}
% EDIT END
\cleardoublepage
}
}
\newcommand{\sectionbreak}
{
\cleardoublepage
\ifthenelse{\boolean{Dipl@IsAppendix}}{\relax}{\suppressfloats}
}
\let\Dipl@OldAppendix\appendix
\renewcommand{\appendix}
{
\Dipl@OldAppendix
\addcontentsline{toc}{section}{\appendixname}%
}
\newcommand{\Dipl@LanguageDependentText}[3]{\ifthenelse{\value{Dipl@CurrentThesisLanguage}=\Dipl@CzechLanguage}{#1}{\ifthenelse{\value{Dipl@CurrentThesisLanguage}=\Dipl@SlovakLanguage}{#2}{#3}}}
\newcommand{\Dipl@AddTokens}[2]{\expandafter \global#1\expandafter{\the#1#2}}
\newcommand{\Dipl@InsertFullPageImage}[1]
{
\newgeometry{margin=1pt,nohead,nofoot,nomarginpar,centering}
\thispagestyle{empty}
\begin{center}
\includegraphics[width=0.99\textwidth,height=0.99\textheight]{#1}
\end{center}
\cleardoublepage
\restoregeometry
}
\newcommand{\Dipl@SetDefaultLanguage}
{
\ifthenelse{\equal{\the\Dipl@DefaultSourceCodeLanguage}{ansic}}{\lstset{language=[ANSI]C}}{}
\ifthenelse{\equal{\the\Dipl@DefaultSourceCodeLanguage}{bash}}{\lstset{language=bash}}{}
\ifthenelse{\equal{\the\Dipl@DefaultSourceCodeLanguage}{cil}}{\lstset{language=CIL}}{}
\ifthenelse{\equal{\the\Dipl@DefaultSourceCodeLanguage}{cpp}}{\lstset{language=C++}}{}
\ifthenelse{\equal{\the\Dipl@DefaultSourceCodeLanguage}{csharp}}{\lstset{language=[Sharp]C}}{}
\ifthenelse{\equal{\the\Dipl@DefaultSourceCodeLanguage}{delphi}}{\lstset{language=Delphi}}{}
\ifthenelse{\equal{\the\Dipl@DefaultSourceCodeLanguage}{gnuplot}}{\lstset{language=Gnuplot}}{}
\ifthenelse{\equal{\the\Dipl@DefaultSourceCodeLanguage}{html}}{\lstset{language=HTML}}{}
\ifthenelse{\equal{\the\Dipl@DefaultSourceCodeLanguage}{java}}{\lstset{language=Java}}{}
\ifthenelse{\equal{\the\Dipl@DefaultSourceCodeLanguage}{latex}}{\lstset{language=[LaTeX]TeX}}{}
\ifthenelse{\equal{\the\Dipl@DefaultSourceCodeLanguage}{lisp}}{\lstset{language=Lisp}}{}
\ifthenelse{\equal{\the\Dipl@DefaultSourceCodeLanguage}{lua}}{\lstset{language=Lua}}{}
\ifthenelse{\equal{\the\Dipl@DefaultSourceCodeLanguage}{make}}{\lstset{language=make}}{}
\ifthenelse{\equal{\the\Dipl@DefaultSourceCodeLanguage}{mathematica}}{\lstset{language=Mathematica}}{}
\ifthenelse{\equal{\the\Dipl@DefaultSourceCodeLanguage}{matlab}}{\lstset{language=Matlab}}{}
\ifthenelse{\equal{\the\Dipl@DefaultSourceCodeLanguage}{octave}}{\lstset{language=Octave}}{}
\ifthenelse{\equal{\the\Dipl@DefaultSourceCodeLanguage}{pascal}}{\lstset{language=Pascal}}{}
\ifthenelse{\equal{\the\Dipl@DefaultSourceCodeLanguage}{perl}}{\lstset{language=Perl}}{}
\ifthenelse{\equal{\the\Dipl@DefaultSourceCodeLanguage}{php}}{\lstset{language=PHP}}{}
\ifthenelse{\equal{\the\Dipl@DefaultSourceCodeLanguage}{pov}}{\lstset{language=POV}}{}
\ifthenelse{\equal{\the\Dipl@DefaultSourceCodeLanguage}{prolog}}{\lstset{language=Prolog}}{}
\ifthenelse{\equal{\the\Dipl@DefaultSourceCodeLanguage}{python}}{\lstset{language=Python}}{}
\ifthenelse{\equal{\the\Dipl@DefaultSourceCodeLanguage}{ruby}}{\lstset{language=Ruby}}{}
\ifthenelse{\equal{\the\Dipl@DefaultSourceCodeLanguage}{r}}{\lstset{language=R}}{}
\ifthenelse{\equal{\the\Dipl@DefaultSourceCodeLanguage}{sh}}{\lstset{language=sh}}{}
\ifthenelse{\equal{\the\Dipl@DefaultSourceCodeLanguage}{sql}}{\lstset{language=SQL}}{}
\ifthenelse{\equal{\the\Dipl@DefaultSourceCodeLanguage}{tcltk}}{\lstset{language=[tk]tcl}}{}
\ifthenelse{\equal{\the\Dipl@DefaultSourceCodeLanguage}{vb}}{\lstset{language=[Visual]Basic}}{}
\ifthenelse{\equal{\the\Dipl@DefaultSourceCodeLanguage}{vhdl}}{\lstset{language=VHDL}}{}
\ifthenelse{\equal{\the\Dipl@DefaultSourceCodeLanguage}{xml}}{\lstset{language=XML}}{}
\ifthenelse{\equal{\the\Dipl@DefaultSourceCodeLanguage}{xslt}}{\lstset{language=XSLT}}{}
}
\lstset
{
breaklines=true,
tabsize=3,
columns=flexible,
showstringspaces=false,
basicstyle=\ttfamily,
keywordstyle=\color{blue},
identifierstyle=\color[RGB]{43,145,175},
commentstyle=\slshape\color[RGB]{0,128,0},
stringstyle=\color[RGB]{163,21,21},
frame=lines,
captionpos=b,
inputencoding=utf8,
extendedchars=true,
literate={á}{{\'{a}}}1 {Á}{{\'{A}}}1 {ä}{{\"{a}}}1 {Ä}{{\"{A}}}1 {č}{{\v{c}}}1 {Č}{{\v{C}}}1 {ď}{{\v{d}}}1 {Ď}{{\v{D}}}1 {ě}{{\v{e}}}1 {Ě}{{\v{E}}}1 {é}{{\'{e}}}1 {É}{{\'{E}}}1 {í}{{\'{i}}}1 {Í}{{\'{I}}}1 {ĺ}{{\'{l}}}1 {Ĺ}{{\'{L}}}1 {ľ}{{\v{l}}}1 {Ľ}{{\v{L}}}1 {ň}{{\v{n}}}1 {Ň}{{\v{N}}}1 {ó}{{\'{o}}}1 {Ó}{{\'{O}}}1 {ô}{{\^{o}}}1 {Ô}{{\^{O}}}1 {ř}{{\v{r}}}1 {Ř}{{\v{R}}}1 {ŕ}{{\'{r}}}1 {Ŕ}{{\'{R}}}1 {š}{{\v{s}}}1 {Š}{{\v{S}}}1 {ť}{{\v{t}}}1 {Ť}{{\v{T}}}1 {ú}{{\'{u}}}1 {Ú}{{\'{U}}}1 {ů}{{\r{u}}}1 {Ů}{{\r{U}}}1 {ý}{{\'{y}}}1 {Ý}{{\'{Y}}}1 {ž}{{\v{z}}}1 {Ž}{{\v{Z}}}1 
}
\onehalfspacing
\setcounter{tocdepth}{2}
\setcounter{secnumdepth}{5}
\widowpenalty=9000
\clubpenalty=9000
\hypersetup{unicode=true, colorlinks=true, linkcolor=black, urlcolor=black, citecolor=black}
\regtotcounter{figure}
\regtotcounter{table}
\regtotcounter{lstlisting}
\addto{\captionsczech}{\renewcommand{\refname}{Literatura}}
\addto{\captionsslovak}{\renewcommand{\refname}{Literatúra}}
\addto{\captionsczech}{\renewcommand{\appendixname}{Přílohy}}
\addto{\captionsslovak}{\renewcommand{\appendixname}{Prílohy}}
\AtBeginDocument%
{
\shorthandoff{-}
\ifthenelse{\value{Dipl@CurrentThesisLanguage}=\Dipl@CzechLanguage}
{
\selectlanguage{czech}
\def\lstlistlistingname{Seznam výpisů zdrojového kódu}
\def\lstlistingname{Výpis}
}{}
\ifthenelse{\value{Dipl@CurrentThesisLanguage}=\Dipl@EnglishLanguage}
{
\selectlanguage{english}
}{}
\ifthenelse{\value{Dipl@CurrentThesisLanguage}=\Dipl@SlovakLanguage}
{
\selectlanguage{slovak}
\def\lstlistlistingname{Zoznam výpisov zdrojového kódu}
\def\lstlistingname{Výpis}
}{}
\renewcommand{\lstlistoflistings}{\begingroup\tocfile{\lstlistlistingname}{lol}\endgroup}
\Dipl@SetDefaultLanguage
}
\endinput
%%
%% End of file `diploma.cls'.
