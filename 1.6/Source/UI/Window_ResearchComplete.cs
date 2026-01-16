using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Verse;

namespace TriumphantResearch
{
    [HotSwappable]
    public class Window_ResearchComplete : Window
    {
        private ResearchProjectDef project;
        private List<Def> unlockedDefs;
        private int currentIndex = 0;
        private Vector2 scrollPosition = Vector2.zero;
        private static FieldInfo _parentCategoryField;
        private static bool _reflectionInitialized;
        private bool reachedLastItem = false;
        private Dictionary<Def, List<ThingDef>> recipeUsersCache = new Dictionary<Def, List<ThingDef>>();

        public Window_ResearchComplete(ResearchProjectDef project)
        {
            this.project = project;
            unlockedDefs = new List<Def>(project.UnlockedDefs);
            soundAppear = DefsOf.TR_ResearchComplete;
            closeOnClickedOutside = false;
            absorbInputAroundWindow = true;
            forcePause = true;
            doCloseX = true;
        }

        public override Vector2 InitialSize => new Vector2(900f, 600f);

        public override void DoWindowContents(Rect inRect)
        {
            float bottomSectionHeight = 150f;
            float topSectionHeight = 60f;
            Rect topRect = new Rect(inRect.x, inRect.y, inRect.width, topSectionHeight);
            Rect bottomRect = new Rect(inRect.x, inRect.yMax - bottomSectionHeight, inRect.width, bottomSectionHeight);
            Rect carouselRect = new Rect(inRect.x, topRect.yMax, inRect.width, inRect.height - topRect.height - 120f);

            DrawTop(topRect);
            DrawCarousel(carouselRect);
            DrawBottom(bottomRect);
        }

        private void DrawTop(Rect topRect)
        {
            float titleHeight = 28f;
            float titleWidth = 250;
            float canMakeHeight = 30f;

            Text.Font = GameFont.Medium;
            Rect titleRect = new Rect(topRect.x, topRect.y, titleWidth, titleHeight);
            Widgets.Label(titleRect, "TR_ResearchComplete".Translate());
            var researchHeight = Text.CalcHeight(project.LabelCap, titleWidth);
            Rect researchNameRect = new Rect(titleRect.x, titleRect.yMax, titleWidth, researchHeight);
            Widgets.Label(researchNameRect, project.LabelCap);

            Rect canMakeRect = new Rect(topRect.x, titleRect.yMax + 30, topRect.width, canMakeHeight);
            Text.Anchor = TextAnchor.UpperCenter;
            Widgets.Label(canMakeRect, "TR_CanNowMake".Translate());
            Text.Anchor = TextAnchor.UpperLeft;
            Text.Font = GameFont.Small;
        }

        private void DrawCarousel(Rect rect)
        {
            float currentItemSize = 260f;
            float nextItemSize = 200f;
            float itemSpacing = 20f;
            float navButtonWidth = 40f;
            float navButtonHeight = currentItemSize;
            float viewAreaHorizontalMargin = navButtonWidth + 10f;
            float iconContractMargin = 8f;
            float scrollSpeed = 0.1f;
            Color nonSelectedColor = Color.black.WithAlpha(0.5f);
            float viewWidth = rect.width - (viewAreaHorizontalMargin * 2);
            Rect viewRect = new Rect(viewAreaHorizontalMargin, rect.y, viewWidth, rect.height);
            float labelHeight = 35f;
            float maxElementHeight = currentItemSize + labelHeight;
            float contentOffsetY = (rect.height - maxElementHeight) / 2f;
            if (contentOffsetY < 0f)
            {
                contentOffsetY = 0f;
            }

            float itemsTotalWidth = 0f;
            float currentItemStartX = 0f;
            for (int i = 0; i < unlockedDefs.Count; i++)
            {
                float size = (i == currentIndex) ? currentItemSize : nextItemSize;
                if (i < currentIndex)
                {
                    currentItemStartX += size + itemSpacing;
                }
                itemsTotalWidth += size + itemSpacing;
            }
            if (unlockedDefs.Any())
            {
                itemsTotalWidth -= itemSpacing;
            }

            float sidePadding = (viewWidth - currentItemSize) / 2f;
            Rect contentRect = new Rect(0f, 0f, itemsTotalWidth + sidePadding * 2, rect.height);

            float targetX = currentItemStartX;
            scrollPosition.x = Mathf.Lerp(scrollPosition.x, targetX, scrollSpeed);
            Widgets.BeginScrollView(viewRect, ref scrollPosition, contentRect, false);

            float currentX = sidePadding;
            for (int i = 0; i < unlockedDefs.Count; i++)
            {
                float itemSize = (i == currentIndex) ? currentItemSize : nextItemSize;
                float itemY = (i == currentIndex) ? contentOffsetY : contentOffsetY + (currentItemSize - nextItemSize) / 2f;
                Rect itemRect = new Rect(currentX, itemY, itemSize, itemSize);

                Widgets.DrawMenuSection(itemRect);

                Widgets.DefIcon(itemRect.ContractedBy(iconContractMargin), unlockedDefs[i]);
                if (i == currentIndex)
                {
                    Def currentDef = unlockedDefs[i];
                    Rect labelRect = new Rect(itemRect.x, itemRect.yMax, itemRect.width, labelHeight);
                    Text.Anchor = TextAnchor.UpperCenter;
                    Text.Font = GameFont.Medium;
                    Widgets.Label(labelRect, currentDef.LabelCap);
                    Text.Font = GameFont.Small;
                    Text.Anchor = TextAnchor.UpperLeft;
                    Widgets.InfoCardButton(itemRect.xMax, itemRect.yMax, currentDef);
                }
                else
                {
                    Widgets.DrawBoxSolid(itemRect.ExpandedBy(1), nonSelectedColor);
                }

                currentX += itemSize + itemSpacing;
            }

            Widgets.EndScrollView();

            if (unlockedDefs.Count > 1)
            {
                float navButtonY = rect.y + (rect.height / 2f) - (navButtonWidth / 2f);
                Rect prevButtonRect = new Rect(0f, navButtonY - navButtonHeight / 2f, navButtonWidth, navButtonHeight);
                if (currentIndex > 0 && Widgets.ButtonText(prevButtonRect, "<"))
                {
                    currentIndex--;
                }

                Rect nextButtonRect = new Rect(rect.width - navButtonWidth, navButtonY - navButtonHeight / 2f, navButtonWidth, navButtonHeight);
                if (currentIndex < unlockedDefs.Count - 1 && Widgets.ButtonText(nextButtonRect, ">"))
                {
                    currentIndex++;
                    if (currentIndex == unlockedDefs.Count - 1 || currentIndex >= 14)
                    {
                        reachedLastItem = true;
                    }
                }
            }
        }

        private void DrawBottom(Rect bottomRect)
        {
            float descriptionWidth = bottomRect.width - 400;
            float doneButtonWidth = 100f;

            Def currentDef = unlockedDefs[currentIndex];

            float textHeight = bottomRect.height;
            float textY = bottomRect.yMax - textHeight;

            Rect createdInRect = new Rect(bottomRect.x, textY, bottomRect.width, textHeight - 35);
            Rect descriptionRect = new Rect(bottomRect.x + 220, textY, descriptionWidth, textHeight - 35);

            DrawProductionInfo(createdInRect, currentDef);

            Widgets.LabelScrollable(descriptionRect, currentDef.description, ref descriptionScrollable);

            if (reachedLastItem || unlockedDefs.Count == 1)
            {
                Rect doneButtonRect = new Rect(bottomRect.xMax - doneButtonWidth, bottomRect.yMax - 30, doneButtonWidth, 30f);
                if (Widgets.ButtonText(doneButtonRect, "TR_Done".Translate()))
                {
                    Close();
                }
            }
        }
        public static Vector2 descriptionScrollable;

        private static void InitializeReflection()
        {
            if (_reflectionInitialized) return;

            var extensionType = GenTypes.GetTypeInAnyAssembly("BetterArchitect.NestedCategoryExtension");
            if (extensionType != null)
            {
                _parentCategoryField = extensionType.GetField("parentCategory", BindingFlags.Instance | BindingFlags.Public);
            }
            _reflectionInitialized = true;
        }

        private string GetCategoryPath(DesignationCategoryDef category)
        {
            InitializeReflection();

            if (_parentCategoryField == null || category.modExtensions == null) return category.label;

            foreach (var extension in category.modExtensions)
            {
                if (extension.GetType().Name == "NestedCategoryExtension")
                {
                    var parentCategory = _parentCategoryField.GetValue(extension) as DesignationCategoryDef;
                    if (parentCategory != null)
                    {
                        return GetCategoryPath(parentCategory) + " > " + category.label;
                    }
                }
            }

            return category.label;
        }

        private void DrawProductionInfo(Rect rect, Def def)
        {
            if (def is ThingDef thingDef)
            {
                if (!recipeUsersCache.TryGetValue(def, out List<ThingDef> recipeUsers))
                {
                    IEnumerable<RecipeDef> recipes = DefDatabase<RecipeDef>.AllDefsListForReading.Where((RecipeDef r) => r.products.Count == 1 && r.products.Any((ThingDefCountClass p) => p.thingDef == thingDef) && !r.IsSurgery);
                    if (recipes.Any())
                    {
                        recipeUsers = recipes.Where((RecipeDef x) => x.recipeUsers != null).SelectMany((RecipeDef r) => r.recipeUsers)
                            .Concat(from x in DefDatabase<ThingDef>.AllDefsListForReading
                                    where x.recipes != null && x.recipes.Any((RecipeDef y) => y.products.Any((ThingDefCountClass z) => z.thingDef == thingDef))
                                    select x).Distinct().ToList();
                        recipeUsersCache[def] = recipeUsers;
                    }
                    else
                    {
                        recipeUsersCache[def] = new List<ThingDef>();
                        recipeUsers = recipeUsersCache[def];
                    }
                }

                if (recipeUsers.Any())
                {
                    float iconSize = 24f;
                    float lineHeight = Text.LineHeight;
                    float curY = rect.y;

                    Text.Font = GameFont.Small;
                    Text.Anchor = TextAnchor.MiddleLeft;

                    Widgets.Label(new Rect(rect.x, curY, rect.width, lineHeight), "TR_CreatedIn".Translate());
                    curY += lineHeight;

                    foreach (ThingDef recipeUser in recipeUsers)
                    {
                        Rect iconRect = new Rect(rect.x, curY, iconSize, iconSize);
                        float labelWidth = Text.CalcSize(recipeUser.LabelCap).x + 5;
                        Rect labelRect = new Rect(rect.x + iconSize + 4f, curY, labelWidth, lineHeight);
                        Rect clickRect = new Rect(iconRect.x - 5, iconRect.y, iconSize + labelWidth + 10, iconRect.height).ContractedBy(2);

                        Widgets.DefIcon(iconRect, recipeUser);
                        Widgets.Label(labelRect, recipeUser.LabelCap);

                        if (Mouse.IsOver(clickRect))
                        {
                            Widgets.DrawHighlight(clickRect);
                        }
                        if (Widgets.ButtonInvisible(clickRect))
                        {
                            Find.WindowStack.Add(new Dialog_InfoCard(recipeUser));
                        }

                        curY += lineHeight;
                    }

                    Text.Anchor = TextAnchor.UpperLeft;
                    Text.Font = GameFont.Small;
                    return;
                }
            }

            if (def is BuildableDef buildable && buildable.designationCategory != null)
            {
                float lineHeight = Text.LineHeight;
                float curY = rect.y;

                string categoryPath = ModsConfig.IsActive("ferny.BetterArchitect")
                    ? GetCategoryPath(buildable.designationCategory)
                    : buildable.designationCategory.label;

                Text.Font = GameFont.Small;
                Text.Anchor = TextAnchor.MiddleLeft;

                Widgets.Label(new Rect(rect.x, curY, rect.width, lineHeight), "TR_CreatedIn".Translate());
                curY += lineHeight;

                Widgets.Label(new Rect(rect.x, curY, rect.width, lineHeight), "TR_ArchitectMenu".Translate());
                curY += lineHeight;

                Rect labelRect = new Rect(rect.x + 4f, curY, rect.width - 4f, lineHeight);

                Widgets.Label(labelRect, categoryPath.CapitalizeFirst());

                Text.Anchor = TextAnchor.UpperLeft;
                Text.Font = GameFont.Small;
            }
        }

        public override void PostClose()
        {
            base.PostClose();
            if (ModsConfig.IsActive("arodoid.semirandomprogression") && ResearchManager_FinishProject_Patch.adding is false)
            {
                var semiRandomResearchButton = DefDatabase<MainButtonDef>.GetNamed("CM_Semi_Random_Research_MainButton_Next_Research", false);
                if (semiRandomResearchButton != null && Find.MainTabsRoot != null)
                {
                    MainTabsRoot_SetCurrentTab_Patch.allow = true;
                    Find.MainTabsRoot.SetCurrentTab(semiRandomResearchButton, true);
                    MainTabsRoot_SetCurrentTab_Patch.allow = false;
                }
            }
        }
    }
}
